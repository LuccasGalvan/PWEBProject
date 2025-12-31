using Microsoft.AspNetCore.Components;
using RCLAPI.DTO;
using RCLAPI.Services;

namespace RCLProdutos.Shared.Cards
{
    public partial class CardsCarousel
    {
        [Inject]
        public IApiServices _apiServices { get; set; }

        [Parameter]
        public IEnumerable<Categoria>? Categories { get; set; }

        [Parameter]
        public EventCallback<Categoria> OnSelect { get; set; }

        [Parameter]
        public int? SelectedCategoryId { get; set; }

        [Parameter]
        public string? Title { get; set; }

        private List<Categoria>? categorias { get; set; }
        private List<string> marginLeftSlide = new();
        private int countSlide = 0;
        private bool IsDisabledNext { get; set; } = false;
        private bool IsDisbledPrevious { get; set; } = false;

        protected override async Task OnParametersSetAsync()
        {
            if (Categories is not null)
            {
                categorias = Categories.ToList();
            }
            else if (categorias == null)
            {
                categorias = await _apiServices.GetCategorias();
            }

            if (categorias != null)
            {
                await LoadMarginsLeft();
                int qtdProd = categorias.Count;
            }
            else
            {
                Console.WriteLine("Failed to load categories.");
            }
        }

        async Task LoadMarginsLeft()
        {
            if (categorias == null)
            {
                // Handle the null case, maybe log an error or initialize categorias
                return;
            }

            countSlide = 0;
            marginLeftSlide.Clear();
            foreach (var categoria in categorias)
            {
                marginLeftSlide.Add("margin-left:0%");
            }
        }

        void PreviousCard()
        {
            if (marginLeftSlide.Count == 0)
            {
                IsDisbledPrevious = true;
                IsDisabledNext = true;
                return;
            }

            if (countSlide != 0)
            {
                marginLeftSlide[countSlide - 1] = "margin-left:0%";
                countSlide--;
                IsDisabledNext = false;
                IsDisbledPrevious = false;
            }
            else
            {
                marginLeftSlide[0] = "margin-left:0%";
                IsDisbledPrevious = true;
            }
        }

        void NextCard()
        {
            if (marginLeftSlide.Count == 0)
            {
                IsDisbledPrevious = true;
                IsDisabledNext = true;
                return;
            }

            countSlide++;
            if (countSlide < marginLeftSlide.Count)
            {
                marginLeftSlide[countSlide - 1] = $"margin-left:-7%";
                IsDisabledNext = false;
                IsDisbledPrevious = false;
            }
            else
            {
                IsDisabledNext = true;
            }
        }

        private string GetMarginLeft(int index)
        {
            if (index < 0 || index >= marginLeftSlide.Count)
            {
                return "margin-left:0%";
            }

            return marginLeftSlide[index];
        }
    }
}
