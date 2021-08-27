using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ModuleCore.UserControls.DiagramDesigner
{
    public class MoveThumb : Thumb
    {
        private RotateTransform rotateTransform;
        private ContentControl designerItem;

        public MoveThumb()
        {
            DragStarted += new DragStartedEventHandler(MoveThumb_DragStarted);
            DragDelta += new DragDeltaEventHandler(MoveThumb_DragDelta);
        }

        private void MoveThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            designerItem = DataContext as ContentControl;

            if (designerItem != null)
            {
                rotateTransform = designerItem.RenderTransform as RotateTransform;
            }
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double x = Canvas.GetLeft(designerItem);
            double y = Canvas.GetTop(designerItem);

            if (designerItem != null)
            {
                Point dragDelta = new(e.HorizontalChange, e.VerticalChange);

                var rrr = (designerItem.Parent as Canvas).Parent as RotateRectROI;
                if (rrr is not null)
                {
                    rrr.CenterX = (x + dragDelta.X + x + dragDelta.X + designerItem.Width) / 2;
                    rrr.CenterY = (y + dragDelta.Y + y + dragDelta.Y + designerItem.Height) / 2;
                }

                if (rotateTransform != null)
                {
                    dragDelta = rotateTransform.Transform(dragDelta);
                }

                Canvas.SetLeft(designerItem, x + dragDelta.X);
                Canvas.SetTop(designerItem, y + dragDelta.Y);
            }
        }
    }
}