## 简单介绍：
看这篇博客快速了解：
https://blog.csdn.net/tfarcraw/article/details/108028209?spm=1001.2014.3001.5501
 ![avatar](main.jpg),
 

目标框架：.NET5.0

使用到的开源组件：
 
OpenCvSharp  https://github.com/shimat/opencvsharp

Prism    https://prismlibrary.com/

Material Design In XAML Toolkit   http://www.materialdesigninxaml.net/   

Newtonsoft Json.NET  http://www.materialdesigninxaml.net/

NLog  https://nlog-project.org/?r=redirect


交流QQ群：827888895

## 推荐关注另一个opencvsharp开源项目：https://gitee.com/lolo77/OpenCVVision，这个项目演示了3D结构光，光度立体法等一些酷炫的应用，非常棒！

## 更新日志：
2020.08.27：

1、调整程序框架；

2、不再集成相机运行时，如果需要请另行下载，下载后解压至D:\OpenCV\Runtimes目录下：
：

海康SDK（如果已经安装MVS就不需要下载）:

链接：https://pan.baidu.com/s/1BZ_flwm3OjcG6-5XpflzTw 
提取码：yiji


Basler:（如果已经安装pylon就不需要下载）：

链接：https://pan.baidu.com/s/1VkwENHA7wusdosoKpaFpBw 
提取码：yiji

2020.08.29
整理opnecvcli分支，加入zbar条码解码

捐助本项目后联系我:wechat:17551023102 提供opnecvcli的源码。

opencv 4.5.2运行时(也可以自己编译）：

链接：https://pan.baidu.com/s/1TQnZJrvFYk8AqoDgZFRBaA 
提取码：yiji

zbarX64运行时:

链接：https://pan.baidu.com/s/1ZYkwkvoq8oI6jml8UQVLUA 
提取码：yiji

运行时文件的放置目录应该与App.xaml.cs中的这句设置一致：

```csharp
//设置程序的运行时环境
System.Environment.SetEnvironmentVariable("Path", @"D:/OpenCV/Runtimes/opencv452/;D:/OpenCV/Runtimes/zbarX64Runtime/;D:/OpenCV/Runtimes/BaslerRuntimeX64/;D:/OpenCV/Runtimes/MVSRuntimeX64/;C:\Program Files\Basler\pylon 6\Runtime\x64;C:\Program Files (x86)\Common Files\MVS\Runtime\Win64_x64;");
```