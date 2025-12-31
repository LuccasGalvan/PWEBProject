using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;
using RCLAPI.DTO;
using RCLAPI.Services;
using RCLProdutos.Services.Interfaces;
using System;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using Xamarin.Essentials;

namespace RCLProdutos.Shared.Slider;

public partial class SlideComponent
{

    [SupplyParameterFromQuery]
    public int prodSugereId { get; set; }
    
    [Parameter]
    public ProdutoDTO? produto { get; set; } = new ProdutoDTO();

    [Parameter]
    public float? width { get; set; }

    [Parameter]
    public string? marginLeft { get; set; }

    [Inject]
    public IApiServices? _apiServices { get; set; }
    [Inject]
    public IJSRuntime JSRuntime { get; set; }
    public int countSlide { get; set; } = 0;

    public ProdutoFavorito? produtoFavorito { get; set; } = new ProdutoFavorito();
    private string? uidprod { get; set; }
    private string? favoritoicon { get; set; }
    private string? pathurlimg { get; set; }
    private string? favoritoAuthMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        modalDisplay2 = "none";
        modalDisplay1 = "none";
       
        mostraInfo = "none";
        fazcompra = "none";
        if (!produto.Favorito)
            favoritoicon = $"images/heart.png";
        else
        {
            favoritoicon = $"images/heartfilltransp.png";
        }
    }

    private string mostraInfo;
    private string fazcompra;

    private string acao;
    private string acaocompra;
    private void Info()
    {
        acao = mostraInfo;

        if (acao == "none")
            mostraInfo = "block";
        else mostraInfo = "none";
    }

    private void Comprar()
    {
        acaocompra = fazcompra;

        if (acaocompra == "none")
            fazcompra = "block";
        else fazcompra = "dispose";
    }

    //*************** MODAIS ************

    private string modalDisplay1 = "none;";
    private string modalDisplay2 = "none;";

    private string modalClass = string.Empty;

    private int quantidade = 0;
    private decimal total = 0;
    public string limiteQtd = "";

    private bool abreModal1 = false;
    private bool abreModal2 = false;

    //public async void AbreFecha(string janela1, string janela2)
    //{
    //    if (janela1 == "abre")
    //    {
    //        modalDisplay1 = "block";
    //        abreModal1 = true;
    //    }
    //    else if (janela1 == "fecha")
    //    {
    //        modalDisplay1 = "none";
    //        abreModal1 = false;
    //    }
    //    else if (janela1 == "grava")
    //    {
    //        modalDisplay2 = "none";
    //        abreModal2 = false;

    //        var carrinhoCompra = new ItemCarrinhoCompra()
    //        {
    //            Quantidade = quantidade,
    //            PrecoUnitario = produto.Preco,
    //            ValorTotal = total,
    //            ProdutoId = produto.Id,
    //            UserId = "user"
    //        };

    //        var response = await _apiServices.AdicionaItemNoCarrinho(carrinhoCompra);

    //    }

    //    if (janela2 == "abre")
    //    {
    //        modalDisplay2 = "block";
    //        abreModal2 = true;
    //    }
    //    else if (janela2 == "fecha")
    //    {
    //        modalDisplay2 = "none";
    //        abreModal2 = false;
    //        quantidade = 0;
    //    }

    //}

    public async void AbreFecha(string janela1, string janela2)
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
        else if (janela1 == "grava")
        {
            modalDisplay2 = "none";
            abreModal2 = false;

            if (quantidade > 0)
            {
                var cartUserId = await GetCartUserIdAsync();
                var response = await _apiServices.AtualizarCarrinho(cartUserId, produto.Id, "adicionar", quantidade);

                if (response.Success)
                {
                    Console.WriteLine("Item adicionado ao carrinho com sucesso!");
                }
                else
                {
                    Console.WriteLine($"Erro ao adicionar item ao carrinho: {response.Message}");
                }
            }
            else
            {
                Console.WriteLine("Quantidade inválida para adicionar ao carrinho.");
            }

            quantidade = 0;
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

    private async Task<string> GetCartUserIdAsync()
    {
        var cartUserId = await JSRuntime.InvokeAsync<string>("localStorage.getItem", new object?[] { "cartUserId" });
        if (!string.IsNullOrEmpty(cartUserId))
        {
            return cartUserId;
        }

        var userId = await JSRuntime.InvokeAsync<string>("localStorage.getItem", new object?[] { "userID" });
        if (!string.IsNullOrEmpty(userId))
        {
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", new object?[] { "cartUserId", userId });
            return userId;
        }

        var guestId = $"guest_{Guid.NewGuid()}";
        await JSRuntime.InvokeVoidAsync("localStorage.setItem", new object?[] { "cartUserId", guestId });
        return guestId;
    }
    public async void Favoritos(string acao, int pId)
    {
        favoritoAuthMessage = null;
        if (favoritoicon == $"images/heart.png")
        {
            favoritoicon = $"images/heartfilltransp.png";
            var (sucesso, mensagemErro) = await _apiServices.ActualizaFavorito("heartfill", pId);
            if (!sucesso)
            {
                HandleFavoritoError(mensagemErro, "images/heart.png");
            }
        }
        else
        {
            favoritoicon = $"images/heart.png";
            var (sucesso, mensagemErro) = await _apiServices.ActualizaFavorito("heartsimples", pId);
            if (!sucesso)
            {
                HandleFavoritoError(mensagemErro, "images/heartfilltransp.png");
            }
        }
    }

    private void HandleFavoritoError(string? mensagemErro, string iconFallback)
    {
        if (IsAuthError(mensagemErro))
        {
            favoritoAuthMessage = "Inicie sessão para gerir favoritos.";
        }
        else
        {
            favoritoAuthMessage = "Não foi possível atualizar os favoritos.";
        }

        favoritoicon = iconFallback;
    }

    private static bool IsAuthError(string? errorMessage)
    {
        return errorMessage == "Unauthorized" || errorMessage == "Forbidden";
    }

    public void Incrementa(string incredec, string janela2)
    {
        if (incredec == "incrementa")
        {
            quantidade++;
            if(quantidade > produto.EmStock)
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

        total = quantidade * produto.Preco;
    }
}
