using RCLProdutos.Services.Interfaces;

namespace RCLProdutos.Services
{
    public class SliderUtilsServices : ISliderUtilsServices
    {
        public int _value { get; set; } = 0;
        public int Index {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                NotificationOnChange();
            }
        
        }

        private int _valueCont { get; set; } = 0;
        public int CountSlide
        {
           get
           {
                return _valueCont;
           }
           set
           {
                _valueCont = value;
                NotificationOnChange();
           }

        }

        private float _valueWidthSlide2 { get; set; } = 0.00f;

        public float WidthSlide2 
        {
            get
            {
                return _valueWidthSlide2;
            }
            set
            {
                _valueWidthSlide2 = value;
                NotificationOnChange();
            }
        }

        public List<string> _marginLeftSlide = new List<string>();

        public List<string> MarginLeftSlide {
            get 
            {
                return _marginLeftSlide;
            }
            set
            {
                _marginLeftSlide = value;
                NotificationOnChange();
            } 
        }

        public event Action OnChange;
        private void NotificationOnChange() => OnChange?.Invoke();
    }
}
