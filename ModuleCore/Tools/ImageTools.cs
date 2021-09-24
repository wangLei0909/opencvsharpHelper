using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ModuleCore.Common
{
    /// <summary>
    /// 图标操作类
    /// </summary>
    public class ImageTools
    {
        /// <summary>
        /// 获取背景预览图
        /// </summary>
        /// <returns></returns>
        public static async Task<ObservableCollection<SkinNode>> GetPrewView()
        {
            return await Task.Run(() =>
            {
                string path = $"{AppDomain.CurrentDomain.BaseDirectory}Skin\\Preview";
                if (Directory.Exists(path))
                {
                    DirectoryInfo info = new DirectoryInfo(path);
                    var ifs = info.GetFiles()?.ToList();
                    ObservableCollection<SkinNode> skins = new ObservableCollection<SkinNode>();
                    ifs.ForEach(arg =>
                    {
                        skins.Add(new SkinNode()
                        {
                            Name = arg.Name,
                            Image = ImageTools.ConvertToImage(arg.FullName)
                        });
                    });
                    return skins;
                }
                return null;
            });
        }

        public static BitmapImage ConvertToImage(string fileName)
        {
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new System.Uri(fileName);
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
    }

    /// <summary>
    /// 样式节点
    /// </summary>
    public struct SkinNode
    {
        /// <summary>
        /// 图名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 预览图
        /// </summary>
        public BitmapImage Image { get; set; }
    }
}