using Microsoft.AspNetCore.Components;
using RCLAPI.DTO;
using RCLAPI.Services;
using RCLProdutos.Services.Interfaces;

namespace RCLProdutos.Shared.Cards
{
    public partial class CardsCarousel
    {

        [Parameter]
        public int SelectedId { get; set; }

        [Parameter]
        public int catSel { get; set; }

        [Inject]
        public IApiServices _apiServices { get; set; }

        [Inject]
        public ICardsUtilsServices cardsUtilsServices { get; set; }

        private List<Categoria>? categorias { get; set; }

        private bool IsDisabledNext { get; set; } = false;
        private bool IsDisbledPrevious { get; set; } = false;

        private int SelectCategoria;

        private int enviaCat;

        protected override async Task OnInitializedAsync()
        {
            enviaCat = catSel;
            catSel = 0;
            SelectCategoria = SelectedId;

            categorias = await _apiServices.GetCategorias();

            if (categorias != null)
            {
                await LoadMarginsLeft();
                int qtdProd = categorias.Count;
            }
            else
            {
                // Handle the case when categorias is null
                Console.WriteLine("Failed to load categories.");
            }

            cardsUtilsServices.OnChange += StateHasChanged;

        }

        async Task LoadMarginsLeft()
        {
            if (categorias == null)
            {
                // Handle the null case, maybe log an error or initialize categorias
                return;
            }

            foreach (var categoria in categorias)
            {
                cardsUtilsServices.MarginLeftSlide.Add("margin-left:0%");
            }
        }

        void PreviousCard()
        {
            if (cardsUtilsServices.CountSlide != 0)
            {
                cardsUtilsServices.MarginLeftSlide[cardsUtilsServices.CountSlide - 1] = "margin-left:0%";
                cardsUtilsServices.CountSlide--;
                IsDisabledNext = false;
                IsDisbledPrevious = false;
            }
            else
            {
                cardsUtilsServices.MarginLeftSlide[0] = "margin-lef:0%";
                IsDisbledPrevious = true;
            }
            cardsUtilsServices.Index = cardsUtilsServices.CountSlide;
        }

        void NextCard()
        {
            cardsUtilsServices.CountSlide++;
            cardsUtilsServices.Index = cardsUtilsServices.CountSlide;
            if (cardsUtilsServices.CountSlide < cardsUtilsServices.MarginLeftSlide.Count)
            {
                cardsUtilsServices.MarginLeftSlide[cardsUtilsServices.CountSlide - 1] = $"margin-left:-7%";
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
