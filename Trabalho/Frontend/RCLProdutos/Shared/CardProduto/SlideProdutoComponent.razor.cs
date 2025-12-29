using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RCLAPI.DTO;
using RCLAPI.Services;
using RCLProdutos.Services.Interfaces;
using System.Linq;

namespace RCLProdutos.Shared.CardProduto;
public partial class SlideProdutoComponent
{
    [Parameter]
    public ProdutoDTO sugestProduto { get; set; }
    private string? pathurlimg { get; set; }

    public  bool _entra = false;
    public ProdutoDTO recebe = new ProdutoDTO();
    private int sugestId;
    protected override async Task OnInitializedAsync()
    {

        if (sugestProduto is not null)
        sugestId = sugestProduto.Id;

    }

    //******************* MODAIS ********

    private string modalDisplay1 = "none;";
    private string modalDisplay2 = "none;";

    private string modalClass = string.Empty;

    private decimal quantidade = 0;
    private decimal total = 0;
    public string limiteQtd = "";

    private bool abreModal1 = false;
    private bool abreModal2 = false;

    public void AbreFecha(string janela1, string janela2)
    {
        if (janela1 == "abre")
        {
            modalDisplay1 = "block";
            abreModal1 = true;
        }
        else if (janela1 == "fecha")
        {
            modalDisplay1 = "none";
            abreModal1 = false;
        }

        if (janela2 == "abre")
        {
            modalDisplay2 = "block";
            abreModal2 = true;
        }
        else if (janela2 == "fecha")
        {
            modalDisplay2 = "none";
            abreModal2 = false;
            quantidade = 0;
        }
    }

    public void Incrementa(string incredec, string janela2)
    {
        if (incredec == "incrementa")
        {
            quantidade++;
            if (quantidade > sugestProduto.EmStock)
            {
                quantidade--;

                limiteQtd = "Lamentamos mas não existe mais stock!";
            }
        }

        else if (incredec == "desincrementa" && quantidade > 0)
        {
            quantidade--;
            limiteQtd = "";
        }

        total = quantidade * sugestProduto.Preco;
    }
}
