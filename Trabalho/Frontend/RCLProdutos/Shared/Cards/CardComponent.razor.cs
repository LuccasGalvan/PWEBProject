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

        [Parameter]
        public EventCallback<Categoria> OnSelect { get; set; }

        private string SelectedClass => selectedCatId == categoria?.Id ? "selected" : string.Empty;

        private string CardStyle => categoria?.Imagem is { Length: > 0 }
            ? $"background-image:url(data:image/*;base64,{Convert.ToBase64String(categoria.Imagem)});"
            : string.Empty;

        private async Task HandleSelect(Categoria categoria)
        {
            if (OnSelect.HasDelegate)
            {
                await OnSelect.InvokeAsync(categoria);
                return;
            }

            NavigationManager.NavigateTo($"slider?Id={categoria.Id}&nomeCat={categoria.Nome}");
        }
    }
}
