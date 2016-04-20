using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Meizi
{
    public class SubjectImageSizeConvert : IValueConverter
    {
        public System.Object Convert(System.Object value, Type targetType, System.Object parameter, System.String language)
        {
            double v = (double)value;
            int countInRow = (int)v / 100;
            if (countInRow < 2)
            {
                countInRow = 2;
            }
            var imageWidth = v / countInRow - 5;
            return imageWidth;
        }

        public System.Object ConvertBack(System.Object value, Type targetType, System.Object parameter, System.String language)
        {
            throw new NotImplementedException();
        }
    }

    public class GridImageSizeConvert : IValueConverter
    {
        public System.Object Convert(System.Object value, Type targetType, System.Object parameter, System.String language)
        {
            double v = (double)value;
            int countInRow = (int)v / 200;
            if (countInRow < 2)
            {
                countInRow = 2;
            }
            var imageWidth = v / countInRow - 5;
            return imageWidth;
        }

        public System.Object ConvertBack(System.Object value, Type targetType, System.Object parameter, System.String language)
        {
            throw new NotImplementedException();
        }
    }

}
