using Microsoft.AspNetCore.Components;

using RCLAPI.DTO;

namespace RCLProdutos.Shared.Cards
{
    public partial class CardComponent
    {
        [Parameter]
        public Categoria? categoria { get; set; }

        [Parameter]
        public int? selectedCatId { get; set; }    

        [Parameter]
        public string? marginLeft { get; set; }

        public int? selectedCategoriaId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            selectedCategoriaId = selectedCatId;
        }

        private void Navega(Categoria categoria)
        {
            NavigationManager.NavigateTo($"slider?Id={categoria.Id}&nomeCat={categoria.Nome}");
        }
    }
}
