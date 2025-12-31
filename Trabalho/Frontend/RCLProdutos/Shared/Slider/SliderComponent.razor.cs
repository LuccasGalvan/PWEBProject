using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RCLAPI.DTO;
using RCLAPI.Services;
using RCLProdutos.Services.Interfaces;
using System.Text.Json;

namespace RCLProdutos.Shared.Slider
{
    public partial class SliderComponent
    {
        [SupplyParameterFromQuery]
        public string nomeCat { get; set; }

        [SupplyParameterFromQuery]
        public int Id { get; set; }

        [SupplyParameterFromQuery]
        private int compraSugerida { get; set; }

        [Parameter]
        public int? initProd { get; set; }

        [Inject]
        public IApiServices? _apiServices { get; set; }

        [Inject]
        public ISliderUtilsServices sliderUtilsService { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        private List<ProdutoDTO>? produtos { get; set; }
        private List<ProdutoFavorito>? userFavoritos { get; set; }
        private string? favoritosAuthMessage { get; set; }

        private List<Categoria> categorias = new();
        private Dictionary<int, Categoria> categoriaLookup = new();
        private List<Categoria> categoriaNivel1 = new();
        private List<Categoria> categoriaNivel2 = new();
        private List<Categoria> categoriaNivel3 = new();
        private int? selectedNivel1Id;
        private int? selectedNivel2Id;
        private int? selectedNivel3Id;

        public ProdutoDTO sugestaoProduto = new ProdutoDTO();
        private int witdthPerc { get; set; } = 0;
        private bool IsDisabledNext { get; set; } = false;
        private bool IsDisbledPrevious { get; set; } = false;

        public static int? actualProd = 0;

        protected override async Task OnInitializedAsync()
        {
            await LoadCategoriasAsync();
            await LoadProdutosAsync();

            sliderUtilsService.OnChange += StateHasChanged;
        }

        private async Task LoadCategoriasAsync()
        {
            categorias = await _apiServices!.GetCategorias() ?? new();
            categoriaLookup = new Dictionary<int, Categoria>();
            foreach (var categoria in categorias)
            {
                MapCategoriaTree(categoria);
            }

            UpdateCategoriaNiveis(GetCurrentCategoriaId());
        }

        private void MapCategoriaTree(Categoria categoria)
        {
            categoriaLookup[categoria.Id] = categoria;
            foreach (var child in categoria.Children)
            {
                MapCategoriaTree(child);
            }
        }

        private int? GetCurrentCategoriaId()
        {
            if (Id > 0)
            {
                return Id;
            }

            if (actualProd.HasValue && actualProd.Value > 0)
            {
                return actualProd.Value;
            }

            return null;
        }

        private void UpdateCategoriaNiveis(int? selectedCategoriaId)
        {
            categoriaNivel1 = categorias;
            categoriaNivel2 = new List<Categoria>();
            categoriaNivel3 = new List<Categoria>();
            selectedNivel1Id = null;
            selectedNivel2Id = null;
            selectedNivel3Id = null;

            if (selectedCategoriaId == null || !categoriaLookup.TryGetValue(selectedCategoriaId.Value, out var selecionada))
            {
                return;
            }

            var path = new List<Categoria>();
            var current = selecionada;
            while (current != null)
            {
                path.Add(current);
                if (current.ParentId == null)
                {
                    break;
                }

                if (!categoriaLookup.TryGetValue(current.ParentId.Value, out current))
                {
                    break;
                }
            }

            path.Reverse();
            if (path.Count > 0)
            {
                selectedNivel1Id = path[0].Id;
                categoriaNivel2 = path[0].Children?.ToList() ?? new();
            }

            if (path.Count > 1)
            {
                selectedNivel2Id = path[1].Id;
                categoriaNivel3 = path[1].Children?.ToList() ?? new();
            }

            if (path.Count > 2)
            {
                selectedNivel3Id = path[2].Id;
            }
        }

        private async Task HandleCategoriaSelecionada(Categoria categoria)
        {
            Id = categoria.Id;
            nomeCat = categoria.Nome ?? nomeCat;
            actualProd = categoria.Id;
            UpdateCategoriaNiveis(categoria.Id);
            await LoadProdutosAsync();
        }

        private async Task LoadProdutosAsync()
        {
            int? categoriasenviadaID = GetCurrentCategoriaId();
            string produtosEspecificos = categoriasenviadaID.HasValue ? "categoria" : "todos";

            try
            {
                produtos = await _apiServices!.GetProdutosEspecificos(produtosEspecificos, categoriasenviadaID);

                if (produtos == null || !produtos.Any())
                {
                    throw new Exception("Nenhum produto foi recuperado.");
                }

                var userId = await JSRuntime.InvokeAsync<string>("localStorage.getItem", new object[] { "userID" });

                if (userId != null)
                {
                    var (favoritos, errorMessage) = await _apiServices!.GetFavoritos(userId);
                    if (IsAuthError(errorMessage))
                    {
                        favoritosAuthMessage = "Inicie sessão para ver os seus favoritos.";
                        userFavoritos = new List<ProdutoFavorito>();
                    }
                    else
                    {
                        userFavoritos = favoritos ?? new List<ProdutoFavorito>();
                    }

                    for (int i = 0; i < userFavoritos.Count; i++)
                    {
                        for (int j = 0; j < produtos.Count; j++)
                        {
                            if (produtos[j].Id == userFavoritos[i].ProdutoId)
                            {
                                produtos[j].Favorito = userFavoritos[i].Efavorito;
                            }
                        }
                    }
                }

                Random random = new Random();
                int[]? indices = produtos
                                   .Where(item => item is not null)
                                   .Select(item => item.Id)
                                   .ToArray();

                int sugestaoProdutoId = random.Next(0, produtos.Count - 1);
                sugestaoProduto = produtos[indices[sugestaoProdutoId] - 1];
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Erro ao desserializar JSON: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter produtos: {ex.Message}");
            }

            await LoadMarginsLeft();

            if (produtos == null)
            {
                Console.WriteLine("Erro: produtos é null.");
                return;
            }

            int qtdProd = produtos.Count;
            witdthPerc = qtdProd * 100;

            sliderUtilsService.WidthSlide2 = 100f / qtdProd;
        }

        private static bool IsAuthError(string? errorMessage)
        {
            return errorMessage == "Unauthorized" || errorMessage == "Forbidden";
        }

        async Task LoadMarginsLeft()
        {
            if (produtos == null)
            {
                Console.WriteLine("produtos é null");
                return;
            }

            sliderUtilsService.MarginLeftSlide.Clear();
            sliderUtilsService.CountSlide = 0;
            sliderUtilsService.Index = 0;

            foreach (var produto in produtos)
            {
                sliderUtilsService.MarginLeftSlide.Add("margin-left:0%");
            }
        }

        void PreviousSlide()
        {
            if (sliderUtilsService.CountSlide != 0)
            {
                sliderUtilsService.MarginLeftSlide[sliderUtilsService.CountSlide - 1] = "margin-left:0%";
                sliderUtilsService.CountSlide--;
                IsDisabledNext = false;
                IsDisbledPrevious = false;
            }
            else
            {
                sliderUtilsService.MarginLeftSlide[0] = "margin-left:0%";
                IsDisbledPrevious = true;
            }
            sliderUtilsService.Index = sliderUtilsService.CountSlide;
        }

        void NextSlide()
        {
            sliderUtilsService.CountSlide++;
            sliderUtilsService.Index = sliderUtilsService.CountSlide;
            if (sliderUtilsService.CountSlide < sliderUtilsService.MarginLeftSlide.Count)
            {
                sliderUtilsService.MarginLeftSlide[sliderUtilsService.CountSlide - 1] = $"margin-left:-{sliderUtilsService.WidthSlide2}%";
                IsDisabledNext = false;
                IsDisbledPrevious = false;
            }
            else
            {
                IsDisabledNext = true;
            }
        }
    }
}
