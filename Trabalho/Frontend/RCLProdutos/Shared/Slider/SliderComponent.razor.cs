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
        private List<ProdutoDTO>? produtos { get; set; }
        private List<ProdutoFavorito>? userFavoritos { get; set; }

        public ProdutoDTO sugestaoProduto = new ProdutoDTO();
        private int witdthPerc { get; set; } = 0;
        private bool IsDisabledNext { get; set; } = false;
        private bool IsDisbledPrevious { get; set; } = false;

        public static int? actualProd = 0;

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        protected override async Task OnInitializedAsync()
        {
            int? categoriasenviadaID;
            string? produtosEspecificos;

            if (Id == 0 && actualProd == 0 || nomeCat == "Todos")
            {
                produtosEspecificos = "todos";
                categoriasenviadaID = null;
            }
            else if (actualProd == Id)
            {
                categoriasenviadaID = Id;
                produtosEspecificos = "categoria";
            }
            else
            {
                if (Id > 0)
                {
                    categoriasenviadaID = Id;
                    actualProd = Id;
                    produtosEspecificos = "categoria";
                }
                else
                {
                    categoriasenviadaID = actualProd;
                    produtosEspecificos = "categoria";
                }
            }

            try
            {
                // Tentando obter os produtos da API
                produtos = await _apiServices!.GetProdutosEspecificos(produtosEspecificos, categoriasenviadaID);

                if (produtos == null || !produtos.Any())
                {
                    throw new Exception("Nenhum produto foi recuperado.");
                }

                if (produtos == null)
                {
                    Console.WriteLine("Erro: produtos é null.");
                    return;
                }

                if (!produtos.Any())
                {
                    throw new Exception("Nenhum produto foi recuperado.");
                }

                // Obtem o id do utilizador do local storage
                var userId = await JSRuntime.InvokeAsync<string>("localStorage.getItem", new object[] { "userID" });

                if (userId != null)
                {
                    userFavoritos = await _apiServices!.GetFavoritos(userId);

                    // Atualizando os produtos com os favoritos
                    for (int i = 0; i < userFavoritos.Count; i++)
                    {
                        for (int j = 0; j < produtos.Count; j++)
                        {
                            if (produtos[j].Id == userFavoritos[i].ProdutoId)
                                produtos[j].Favorito = userFavoritos[i].Efavorito;
                        }
                    }
                }

                // Gerando uma sugestão de produto aleatória
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

            // Carregando a margem à esquerda dos slides
            await LoadMarginsLeft();

            if (produtos == null)
            {
                Console.WriteLine("Erro: produtos é null.");
                return;
            }

            int qtdProd = produtos.Count;
            witdthPerc = qtdProd * 100;

            sliderUtilsService.WidthSlide2 = 100f / qtdProd;

            sliderUtilsService.OnChange += StateHasChanged;
        }

        // Método para carregar as margens à esquerda dos slides
        async Task LoadMarginsLeft()
        {
            if (produtos == null)
            {
                Console.WriteLine("produtos é null");
                return;
            }

            foreach (var produto in produtos)
            {
                sliderUtilsService.MarginLeftSlide.Add("margin-left:0%");
            }
        }

        // Método para mover o slide para a esquerda
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

        // Método para mover o slide para a direita
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
