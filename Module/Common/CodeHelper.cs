
//using System.Collections.Generic;
//using System.Drawing;
//using ZXing;
//using ZXing.Aztec;
//using ZXing.Common;
//using ZXing.OneD;
//using ZXing.PDF417;
//using ZXing.QrCode;


//namespace CodeHelper
//{
//    public class CodeHelper
//    {
//        #region 编码
//        public static Bitmap Encode_QR(string content, int width = 100, int margin = 1)
//        {
//            QrCodeEncodingOptions opt = new QrCodeEncodingOptions();
//            opt.DisableECI = true;
//            opt.CharacterSet = "UTF-8";
//            opt.Width = width;
//            opt.Height = width;
//            opt.Margin = margin;

//            BarcodeWriter wr = new BarcodeWriter();
//            wr.Options = opt;
//            wr.Format = BarcodeFormat.QR_CODE;

//            Bitmap bm = wr.Write(content);
//            return bm;
//        }



//        public static Bitmap Encode_PDF_417(string content, int width = 100, int margin = 5)
//        {
//            PDF417EncodingOptions opt = new PDF417EncodingOptions();
//            opt.Width = width;
//            opt.Margin = margin;
//            opt.CharacterSet = "UTF-8";

//            BarcodeWriter wr = new BarcodeWriter();
//            wr.Options = opt;
//            wr.Format = BarcodeFormat.PDF_417;

//            Bitmap bm = wr.Write(content);
//            return bm;
//        }

//        public static Bitmap Encode_AZTEC(string content, int width = 100, int margin = 1)
//        {
//            AztecEncodingOptions opt = new AztecEncodingOptions();
//            opt.Width = width;
//            opt.Height = width;
//            opt.Margin = margin;

//            BarcodeWriter wr = new BarcodeWriter();
//            wr.Options = opt;
//            wr.Format = BarcodeFormat.AZTEC;

//            Bitmap bm = wr.Write(content);
//            return bm;
//        }

//        public static Bitmap Encode_Code_128(string content, int heigt = 40, int margin = 5)
//        {
//            Code128EncodingOptions opt = new Code128EncodingOptions();
//            opt.Height = heigt;
//            opt.Margin = margin;

//            BarcodeWriter wr = new BarcodeWriter();
//            wr.Options = opt;
//            wr.Format = BarcodeFormat.CODE_128;

//            Bitmap bm = wr.Write(content);
//            return bm;
//        }

//        public static Bitmap Encode_Code_39(string content, int height = 30, int margin = 1)
//        {
//            EncodingOptions encodeOption = new EncodingOptions();
//            encodeOption.Height = height;
//            encodeOption.Margin = margin;

//            BarcodeWriter wr = new BarcodeWriter();
//            wr.Options = encodeOption;
//            wr.Format = BarcodeFormat.CODE_39;

//            Bitmap bm = wr.Write(content);
//            return bm;
//        }

//        public static Bitmap Encode_EAN_8(string content, int height = 50, int margin = 1)
//        {
//            EncodingOptions encodeOption = new EncodingOptions();
//            encodeOption.Height = height;
//            encodeOption.Margin = margin;

//            BarcodeWriter wr = new BarcodeWriter();
//            wr.Options = encodeOption;
//            wr.Format = BarcodeFormat.EAN_8;

//            Bitmap bm = wr.Write(content);
//            return bm;
//        }

//        public static Bitmap Encode_EAN_13(string content, int height = 50, int margin = 1)
//        {
//            EncodingOptions encodeOption = new EncodingOptions();
//            encodeOption.Height = height;
//            encodeOption.Margin = margin;

//            BarcodeWriter wr = new BarcodeWriter();
//            wr.Options = encodeOption;
//            wr.Format = BarcodeFormat.EAN_13;

//            Bitmap bm = wr.Write(content);
//            return bm;
//        }
//        #endregion

//        #region 编码重载
//        public static Bitmap Encode_QR(string content)
//        {
//            return Encode_QR(content, 100, 1);
//        }



//        public static Bitmap Encode_PDF_417(string content)
//        {
//            return Encode_PDF_417(content, 100, 5);
//        }

//        public static Bitmap Encode_AZTEC(string content)
//        {
//            return Encode_AZTEC(content, 100, 1);
//        }

//        public static Bitmap Encode_Code_128(string content)
//        {
//            return Encode_Code_128(content, 40, 5);
//        }

//        public static Bitmap Encode_Code_39(string content)
//        {
//            return Encode_Code_39(content, 30, 1);
//        }

//        public static Bitmap Encode_EAN_8(string content)
//        {
//            return Encode_EAN_8(content, 50, 1);
//        }

//        public static Bitmap Encode_EAN_13(string content)
//        {
//            return Encode_EAN_13(content, 50, 1);
//        }
//        #endregion

//        /// <summary>
//        /// 全部编码类型解码
//        /// </summary>
//        /// <param name="bm"></param>
//        /// <returns></returns>
//        public static string Decode(Bitmap bm)
//        {
//            DecodingOptions opt = new DecodingOptions();
//            opt.PossibleFormats = new List<BarcodeFormat>()
//            {
//                BarcodeFormat.QR_CODE,
//                BarcodeFormat.DATA_MATRIX,
//                BarcodeFormat.PDF_417,
//                BarcodeFormat.AZTEC,
//                BarcodeFormat.CODE_128,
//                BarcodeFormat.CODE_39,
//                BarcodeFormat.EAN_8,
//                BarcodeFormat.EAN_13
//            };
//            opt.CharacterSet = "UTF-8";

//            BarcodeReader reader = new BarcodeReader();
//            reader.Options = opt;
//            Result rs = reader.Decode(bm);
//            if (rs != null)
//            {
//                return rs.Text;
//            }

//            return "";
//        }
//    }
//}

