using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RCLAPI.DTO;
using RCLAPI.Services;
using RCLProdutos.Services.Interfaces;
using System;
using System.Globalization;
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

        [Inject]
        public NavigationManager NavigationManager { get; set; }

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
            var encodedNomeCat = Uri.EscapeDataString(nomeCat ?? string.Empty);
            NavigationManager.NavigateTo($"/slider?Id={categoria.Id}&nomeCat={encodedNomeCat}");
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
                    Console.WriteLine("Nenhum produto foi recuperado.");
                    produtos ??= new List<ProdutoDTO>();
                    sugestaoProduto = new ProdutoDTO();
                    await LoadMarginsLeft();
                    witdthPerc = 0;
                    sliderUtilsService.WidthSlide2 = 0;
                    return;
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
                int sugestaoIndex = random.Next(produtos.Count);
                sugestaoProduto = produtos[sugestaoIndex];
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
            if (qtdProd == 0)
            {
                witdthPerc = 0;
                sliderUtilsService.WidthSlide2 = 0;
                return;
            }

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
            if (sliderUtilsService.MarginLeftSlide.Count == 0)
            {
                IsDisbledPrevious = true;
                IsDisabledNext = true;
                return;
            }

            if (sliderUtilsService.CountSlide > 0 && sliderUtilsService.CountSlide <= sliderUtilsService.MarginLeftSlide.Count)
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
            if (sliderUtilsService.MarginLeftSlide.Count == 0)
            {
                IsDisbledPrevious = true;
                IsDisabledNext = true;
                return;
            }

            var maxIndex = sliderUtilsService.MarginLeftSlide.Count - 1;
            if (sliderUtilsService.CountSlide >= maxIndex)
            {
                sliderUtilsService.CountSlide = maxIndex;
                sliderUtilsService.Index = sliderUtilsService.CountSlide;
                IsDisabledNext = true;
                return;
            }

            sliderUtilsService.CountSlide++;
            sliderUtilsService.Index = sliderUtilsService.CountSlide;
            if (sliderUtilsService.CountSlide < sliderUtilsService.MarginLeftSlide.Count)
            {
                var widthValue = sliderUtilsService.WidthSlide2.ToString(CultureInfo.InvariantCulture);
                sliderUtilsService.MarginLeftSlide[sliderUtilsService.CountSlide - 1] = $"margin-left:-{widthValue}%";
                IsDisabledNext = false;
                IsDisbledPrevious = false;
            }
            else
            {
                IsDisabledNext = true;
            }
        }

        private string GetSlideMargin(int index)
        {
            if (index < 0 || index >= sliderUtilsService.MarginLeftSlide.Count)
            {
                return "margin-left:0%";
            }

            return sliderUtilsService.MarginLeftSlide[index];
        }
    }
}
