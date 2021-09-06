using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpencvsharpModule.Common
{
    public static class MatExtension
    {
        //画旋转矩形
        public static void DrawRotatedRect(this Mat mat, RotatedRect rr, Scalar scalar, int thickness = 2)
        {
            var P = rr.Points();
            for (int j = 0; j <= 3; j++)
            {
                Cv2.Line(mat, (Point)P[j], (Point)P[(j + 1) % 4], scalar, thickness);
            }
        }

        public static void FillPolygon(this Mat src, List<Point> points)
        {
            List<List<Point>> polygons = new() { points };
            Cv2.FillPoly(src, polygons, Scalar.White);
        }
        public static void FillPolygon(this Mat src, RotatedRect rr)
        {
            var P = rr.Points().Select(p => new Point(p.X,p.Y)).ToArray();
            var pp = new Point[1][] { P};
            Cv2.FillPoly(src, pp, Scalar.White);
        }
        public static void DrawPolygon(this Mat mat, Point2f[] points, int thickness = 1)
        {
            if (points.Length < 2) return;

            for (int j = 0; j <= points.Length - 1; j++)
            {
                Cv2.Line(mat, (Point)points[j], (Point)points[(j + 1) % points.Length], Scalar.RandomColor(), thickness);
            }
        }
        public static void DrawPolygon(this Mat mat, List<Point> points, int thickness = 1)
        {
            if (points.Count < 2) return;

            for (int j = 0; j <= points.Count - 1; j++)
            {
                Cv2.Line(mat, points[j], points[(j + 1) % points.Count], Scalar.RandomColor(), thickness);
            }
        }
        /// <summary>
        /// 仿射变换
        /// </summary>
        /// <param name="src">输入</param>
        /// <param name="center">中心</param>
        /// <param name="angle">角度</param>
        /// <returns> 返回仿射变换后的完整图形 </returns>
        public static Mat Rotate(this Mat src, float angle)
        {
            // angle 0-360
            while (angle < 0) angle += 360;
            if (angle > 360) angle %= 360;
            Mat dst = new Mat();

            //变换矩阵
            //  cos (angle)  sin(angle)
            //  -sin(angle)  cos(angle)

            Mat rot = Cv2.GetRotationMatrix2D(new(0, 0), angle, 1);

            //X==0 Y==0
            var w1 = rot.At<double>(0, 2);
            var h1 = rot.At<double>(1, 2);

            //Y==0
            var w2 = src.Width * rot.At<double>(0, 0) + rot.At<double>(0, 2);
            var h2 = src.Width * rot.At<double>(1, 0) + rot.At<double>(1, 2);

            //x==0
            var w3 = src.Height * rot.At<double>(0, 1) + rot.At<double>(0, 2);
            var h3 = src.Height * rot.At<double>(1, 1) + rot.At<double>(1, 2);

            var w4 = src.Width * rot.At<double>(0, 0) + src.Height * rot.At<double>(0, 1) + rot.At<double>(0, 2);
            var h4 = src.Width * rot.At<double>(1, 0) + src.Height * rot.At<double>(1, 1) + rot.At<double>(1, 2);

            Point[] points = { new(w1, h1), new(w2, h2), new(w3, h3), new(w4, h4) };

            Rect rRect = Cv2.BoundingRect(points);
            switch (angle)
            {
                case >= 0 and <= 90:
                    rot.Set<double>(0, 2, 0);
                    rot.Set<double>(1, 2, h1 - h2);
                    break;

                case > 90 and <= 180:
                    rot.Set<double>(0, 2, -w2 + w1);
                    rot.Set<double>(1, 2, rRect.Height);
                    break;

                case > 180 and <= 270:
                    rot.Set<double>(0, 2, rRect.Width);
                    rot.Set<double>(1, 2, h1 - h3);
                    break;

                case > 270:
                    rot.Set<double>(0, 2, w1 - w3);
                    rot.Set<double>(1, 2, 0);
                    break;
            }

            Cv2.WarpAffine(src, dst, rot, rRect.Size);
            return dst;
        }

        public static Mat Rotate(this Mat src, float angle, ref Point point)
        {
            // angle 0-360
            while (angle < 0) angle += 360;
            if (angle > 360) angle %= 360;
            Mat dst = new Mat();

            //变换矩阵
            //  cos (angle)  sin(angle)
            //  -sin(angle)  cos(angle)

            Mat rot = Cv2.GetRotationMatrix2D(new(0, 0), angle, 1);

            //X==0 Y==0
            var w1 = rot.At<double>(0, 2);
            var h1 = rot.At<double>(1, 2);

            //Y==0
            var w2 = src.Width * rot.At<double>(0, 0) + rot.At<double>(0, 2);
            var h2 = src.Width * rot.At<double>(1, 0) + rot.At<double>(1, 2);

            //x==0
            var w3 = src.Height * rot.At<double>(0, 1) + rot.At<double>(0, 2);
            var h3 = src.Height * rot.At<double>(1, 1) + rot.At<double>(1, 2);

            var w4 = src.Width * rot.At<double>(0, 0) + src.Height * rot.At<double>(0, 1) + rot.At<double>(0, 2);
            var h4 = src.Width * rot.At<double>(1, 0) + src.Height * rot.At<double>(1, 1) + rot.At<double>(1, 2);

            Point[] points = { new(w1, h1), new(w2, h2), new(w3, h3), new(w4, h4) };

            Rect rRect = Cv2.BoundingRect(points);
            switch (angle)
            {
                case >= 0 and <= 90:
                    rot.Set<double>(0, 2, 0);
                    rot.Set<double>(1, 2, h1 - h2);
                    break;

                case > 90 and <= 180:
                    rot.Set<double>(0, 2, -w2 + w1);
                    rot.Set<double>(1, 2, rRect.Height);
                    break;

                case > 180 and <= 270:
                    rot.Set<double>(0, 2, rRect.Width);
                    rot.Set<double>(1, 2, h1 - h3);
                    break;

                case > 270:
                    rot.Set<double>(0, 2, w1 - w3);
                    rot.Set<double>(1, 2, 0);
                    break;
            }
            var x = point.X * rot.At<double>(0, 0) + point.Y * rot.At<double>(0, 1) + rot.At<double>(0, 2);
            var y = point.X * rot.At<double>(1, 0) + point.Y * rot.At<double>(1, 1) + rot.At<double>(1, 2);
            point.X = (int)Math.Round(x, 0);
            point.Y = (int)Math.Round(y, 0);
            Cv2.WarpAffine(src, dst, rot, rRect.Size);
            return dst;
        }

        public static void IntersectionPoint(Line2D Line1, Line2D Line2, out Point2d crossPoint)
        {
            // Vx Vy 与直线共线的归一化向量的XY分量,可以理解为线段的一个端点
            // X1 Y1 直线上某点的坐标 可以理解为线段的另一个端点

            //  如果是一条垂直线，计算斜率会发生除0错误，所以对线稍加修改，对结果影响不大
            if (Line1.X1 - Line1.Vx == 0)
            {
                Line1 = new Line2D(Line1.Vx, Line1.Vy, Line1.X1 + 0.1, Line1.Y1);
            }

            if (Line2.X1 - Line2.Vx == 0)
            {
                Line2 = new Line2D(Line2.Vx, Line2.Vy, Line2.X1 + 0.1, Line2.Y1);
            }
            //对于过两个点(Vx，Vy) 和 (X1，Y1)的直线，斜率为k=(Y1-Vy)/(X1-Vx)。
            double k1 = (Line1.Y1 - Line1.Vy) / (Line1.X1 - Line1.Vx);
            double k2 = (Line2.Y1 - Line2.Vy) / (Line2.X1 - Line2.Vx);

            //交点
            crossPoint.X = (k1 * Line1.Vx - Line1.Vy - k2 * Line2.Vx + Line2.Vy) / (k1 - k2);
            crossPoint.Y = (k1 * k2 * (Line1.Vx - Line2.Vx) + k1 * Line2.Vy - k2 * Line1.Vy) / (k1 - k2);
        }

        public static Line2D LineSegmentPoint2Line2D(LineSegmentPoint line)
        {
            return new Line2D(line.P1.X, line.P1.Y, line.P2.X, line.P2.Y);
        }

        public static void DrawAngle(this Mat img, Point2d p0, Point2d p1, Point2d p2, double radius)
        {
            // 计算直线的角度
            double angle1 = Math.Atan2(-(p1.Y - p0.Y), p1.X - p0.X) * 180 / Cv2.PI;
            double angle2 = Math.Atan2(-(p2.Y - p0.Y), p2.X - p0.X) * 180 / Cv2.PI;
            // 计算主轴的角度
            double angle = angle1 <= 0 ? -angle1 : 360 - angle1;
            // 计算圆弧的结束角度
            double end_angle = (angle2 < angle1) ? (angle1 - angle2) : (360 - (angle2 - angle1));
            // 画圆弧
            Cv2.Ellipse(img, (Point)p0, new Size(radius, radius), angle, 0, end_angle, Scalar.RandomColor(), 2);
            //string a = (angle-end_angle).ToString();
            string a = end_angle.ToString("F3");
            Cv2.PutText(img, a, (Point)p0, HersheyFonts.HersheyDuplex, 0.8d, Scalar.Red);
        }

        public static bool GetGrayAndBgr(this Mat src, out Mat gray, out Mat bgr)
        {
            bgr = new(0, 0, MatType.CV_8UC3, Scalar.Black);
            gray = new(0, 0, MatType.CV_8UC1, Scalar.Black);
            if (src.Empty()) return false;
            if (src.Type() != MatType.CV_8UC3 && src.Type() != MatType.CV_8UC1) return false;

            if (src.Type() == MatType.CV_8UC3)
            {
                gray = src.CvtColor(ColorConversionCodes.BGR2GRAY);

                bgr = src.Clone();
            }
            else
            {
                gray = src.Clone();
                bgr = src.CvtColor(ColorConversionCodes.GRAY2BGR);
            }
            return true;
        }

        public static bool GetGray(this Mat src, out Mat gray)
        {
            gray = new(0, 0, MatType.CV_8UC1, Scalar.Black);
            if (src.Empty()) return false;
            if (src.Type() != MatType.CV_8UC3 && src.Type() != MatType.CV_8UC1) return false;

            if (src.Type() == MatType.CV_8UC3)
            {
                gray = src.CvtColor(ColorConversionCodes.BGR2GRAY);
            }
            else
            {
                gray = src.Clone();
            }
            return true;
        }

        public static bool GetBgr(this Mat src, out Mat bgr)
        {
            bgr = new(0, 0, MatType.CV_8UC3, Scalar.Black);

            if (src.Empty()) return false;
            if (src.Type() != MatType.CV_8UC3 && src.Type() != MatType.CV_8UC1) return false;

            if (src.Type() == MatType.CV_8UC3)
            {
                bgr = src.Clone();
            }
            else
            {
                bgr = src.CvtColor(ColorConversionCodes.GRAY2BGR);
            }
            return true;
        }

        // Checks if a matrix is a valid rotation matrix.
        // 检查一个矩阵是否是一个有效的旋转矩阵。
        private static bool IsRotationMatrix(Mat R)
        {
            Mat Rt = new();
            Cv2.Transpose(R, Rt);
            Mat shouldBeIdentity = Rt * R;
            Mat I = Mat.Eye(3, 3, shouldBeIdentity.Type());
            return Cv2.Norm(I, shouldBeIdentity) < 1e-6;
        }

        // https://blog.csdn.net/xiangxianghehe/article/details/102481769
        // Calculates rotation matrix to euler angles  计算旋转矩阵到欧拉角
        public static bool RotationMatrixToEulerAngles(Mat R, out Vec3d euler)
        {
            euler = new();

            if (IsRotationMatrix(R)) return false;

            var sy = Math.Sqrt(R.At<double>(0, 0) * R.At<double>(0, 0) + R.At<double>(1, 0) * R.At<double>(1, 0));

            bool singular = sy < 1e-6;

            if (!singular)
            {
                euler.Item0 = Math.Atan2(R.At<double>(2, 1), R.At<double>(2, 2));
                euler.Item1 = Math.Atan2(-R.At<double>(2, 0), sy);
                euler.Item2 = Math.Atan2(R.At<double>(1, 0), R.At<double>(0, 0));
            }
            else
            {
                euler.Item0 = Math.Atan2(-R.At<double>(1, 2), R.At<double>(1, 1));
                euler.Item1 = Math.Atan2(-R.At<double>(2, 0), sy);
                euler.Item2 = 0;
            }

            return true;
        }

        //基于傅里叶变换的角度检测 https://blog.csdn.net/CSDN131137/article/details/103008744
        public static (double angle, Mat DFT) GetDFTAngle(this Mat src)
        {
            //OpenCV中的DFT对图像尺寸有一定要求，
            //需要用GetOptimalDFTSize方法来找到合适的大小，
            //根据这个大小建立新的图像，把原图像拷贝过去，多出来的部分直接填充0。

            src.GetGray(out Mat gray);

            int width = Cv2.GetOptimalDFTSize(src.Width);
            int height = Cv2.GetOptimalDFTSize(src.Height);
            var padded = new Mat(height, width, MatType.CV_8UC1, Scalar.Black);//扩展后的图像，单通道

            padded[0, src.Height, 0, src.Width] = gray.Clone();

            padded.ConvertTo(padded, MatType.CV_32FC1);

            Mat zeros = new Mat(padded.Size(), MatType.CV_32FC1) * 0f;
            Mat comImg = new();

            Mat[] paddeds = new[] { padded, zeros };
            //Merge into a double-channel image 合并成一个双通道图像
            Cv2.Merge(paddeds, comImg);

            Cv2.Dft(comImg, comImg);

            Cv2.Split(comImg, out Mat[] planes);
            Cv2.Magnitude(planes[0], planes[1], planes[0]);

            //计算幅值，转换到对数尺度(logarithmic scale)
            //Switch to logarithmic scale, for better visual results
            //M2=log(1+M1)
            Mat magMat = planes[0];
            magMat += Scalar.All(1);
            Cv2.Log(magMat, magMat);

            //Crop the spectrum
            //Width and height of magMat should be even, so that they can be divided by 2
            //-2 is 11111110 in binary system, operator & make sure width and height are always even
            magMat = magMat[new Rect(0, 0, magMat.Cols & -2, magMat.Rows & -2)];

            //Rearrange the quadrants of Fourier image,
            //so that the origin is at the center of image,
            //and move the high frequency to the corners
            int cx = magMat.Cols / 2;
            int cy = magMat.Rows / 2;

            Mat q0 = new(magMat, new Rect(0, 0, cx, cy));
            Mat q1 = new(magMat, new Rect(0, cy, cx, cy));
            Mat q2 = new(magMat, new Rect(cx, cy, cx, cy));
            Mat q3 = new(magMat, new Rect(cx, 0, cx, cy));

            Mat tmp = new();
            q0.CopyTo(tmp);
            q2.CopyTo(q0);
            tmp.CopyTo(q2);

            q1.CopyTo(tmp);
            q3.CopyTo(q1);
            tmp.CopyTo(q3);

            //MatType, then to[0,255]
            Cv2.Normalize(magMat, magMat, 0, 1, NormTypes.MinMax);
            Mat magImg = new(magMat.Size(), MatType.CV_8UC1);
            magMat.ConvertTo(magImg, MatType.CV_8UC1, 255, 0);
            //Cv2.ImShow("test", magImg);

            //Turn into binary image
            Mat magThresh = new();
            Cv2.Threshold(magImg, magThresh, 150, 255, ThresholdTypes.Binary);
            //Cv2.ImShow("Threshold", magImg);

            //Find lines with Hough Transformation

            //Mat linImg = new(magImg.Size(), MatType.CV_8UC3);
            var lines = Cv2.HoughLines(magThresh, 1, Cv2.PI / 180, 100, 0, 0);
            int numLines = lines.Length;
            //for (int l = 0; l < numLines; l++)
            //{
            //    float rho = lines[l].Rho, theta = lines[l].Theta;
            //    Point pt1, pt2;
            //    double a = Math.Cos(theta), b = Math.Sin(theta);
            //    double x0 = a * rho, y0 = b * rho;
            //    pt1.X = (int)Math.Round(x0 + 1000 * (-b));
            //    pt1.Y = (int)Math.Round(y0 + 1000 * (a));
            //    pt2.X = (int)Math.Round(x0 - 1000 * (-b));
            //    pt2.Y = (int)Math.Round(y0 - 1000 * (a));
            //    Cv2.Line(linImg, pt1.X, pt1.Y, pt2.X, pt2.Y, Scalar.Blue);
            //}

            //从三个角度中找到真正的角度
            double angel = 0;
            var piThresh = Cv2.PI / 90;
            float pi2 = (float)Cv2.PI / 2;
            for (int l = 0; l < numLines; l++)
            {
                float theta = lines[l].Theta;
                if (Math.Abs(theta) < piThresh || Math.Abs(theta - pi2) < piThresh)
                    continue;
                else
                {
                    angel = theta;
                    break;
                }
            }
            //计算旋转角度
            //图像必须是正方形，
            //使旋转角度可以计算右
            angel = angel < pi2 ? angel : angel - (float)Cv2.PI;
            if (angel != pi2)
            {
                double angelT = src.Rows * Math.Tan(angel) / src.Cols;
                angel = Math.Atan(angelT);
            }
            double angelD = angel * 180 / Cv2.PI;

            return (angelD, magImg);
        }

        public static void PutTextZh(this Mat src, string text, Rect rect, float emSize = 36, System.Drawing.Font font = null, System.Drawing.Brush color = null)
        {
            color ??= System.Drawing.Brushes.Lime;
            font ??= new System.Drawing.Font(new System.Drawing.FontFamily("微软雅黑"), emSize);
            if (rect.Width  == 0 ) rect.Width = 10;
            if (rect.Height == 0 ) rect.Height = 10;

            if (rect.Width > src.Width) rect.Width = src.Width;
            if (rect.Height > src.Height) rect.Height = src.Height;
            if (rect.Width + rect.Left > src.Width) rect.Left = 0;
            if (rect.Height + rect.Top > src.Height) rect.Top = 0;
      
            src[rect] = src[rect].DrawText(text, font, color).ToMat();
        }

        private static System.Drawing.Bitmap DrawText(this Mat mat, string str, System.Drawing.Font font, System.Drawing.Brush color)
        {
            //if (color == System.Drawing.Brushes.Black)
            //{
            //    System.Drawing.Color newcolor = System.Drawing.Color.FromArgb(1, 1, 1);

            //    color = new System.Drawing.SolidBrush(newcolor);
            //}

            System.Drawing.Bitmap bitmap = mat.ToBitmap();
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
            graphics.DrawString(str, font, color, new System.Drawing.PointF(0, 0));
            graphics.Flush();

            return bitmap;
        }

        public static Point GetCenter(this Mat src) => new(src.Width / 2, src.Height / 2);
    }
}