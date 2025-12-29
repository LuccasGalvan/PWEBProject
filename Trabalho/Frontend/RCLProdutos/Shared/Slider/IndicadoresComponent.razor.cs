using Microsoft.AspNetCore.Components;
using RCLAPI.DTO;
using RCLProdutos.Services.Interfaces;

namespace RCLProdutos.Shared.Slider
{
    public partial class IndicadoresComponent
    {
        [Parameter]
        public List<ProdutoDTO> Produtos { get; set; }

        [Inject]
        public ISliderUtilsServices sliderUtilsService { get; set; }

        void Changed(int index)
        {
            sliderUtilsService.Index = index;
            index = index == 0 ? sliderUtilsService.Index : index;
            Console.WriteLine(index);
            SlideSelected(index);
        }

        void SlideSelected(int value)
        {
            if (sliderUtilsService.CountSlide > value)
            {
                for (int i = sliderUtilsService.CountSlide - 1; i >= value; --i)
                {
                    sliderUtilsService.MarginLeftSlide[i] = "margin-left:0%";
                    Console.WriteLine(sliderUtilsService.CountSlide);
                    sliderUtilsService.CountSlide--;
                }
            }
            else
            {
                for (sliderUtilsService.CountSlide = sliderUtilsService.CountSlide;
                    sliderUtilsService.CountSlide < value;
                    sliderUtilsService.CountSlide++)
                {
                    string WidthSlideS = (Convert.ToString(sliderUtilsService.WidthSlide2));

                    WidthSlideS = WidthSlideS.Replace(",", ".");

                    sliderUtilsService.MarginLeftSlide[sliderUtilsService.CountSlide] = $"margin-left:-{WidthSlideS}%";
                }
            }
        }

    }
}
