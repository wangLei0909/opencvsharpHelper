using ModuleCore.Mvvm;

using Natasha.CSharp;

using OpenCvSharp;

using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace OpencvsharpModule.Models
{
    public class RoslynEditorModel : BindableBase
    {
        public ImagePool Pool { get; set; }

        public RoslynEditorModel(IContainerExtension container, IEventAggregator ea)
        {
            _ea = ea;
            Pool = container.Resolve<ImagePool>();
            NatashaInit();
        }

        #region Natasha

        async private void NatashaInit()
        {
            //注册+预热组件 , 之后编译会更加快速
            await NatashaInitializer.Initialize();
            if (!File.Exists(@"./Scripts/run/run.cscript"))
                File.Copy(@"./Scripts/new.cscript", @"./Scripts/run/run.cscript");
            ScriptCode = File.ReadAllText(@"./Scripts/run/run.cscript");
            FileName = @"./Scripts/run/run.cscript";
            DirectoryInfo folder = new DirectoryInfo(@"./Scripts/run");

            foreach (FileInfo file in folder.GetFiles())
            {
                Files[file.Name] = file.FullName;
            }
        }

        private AssemblyCSharpBuilder sharpBuilder;
        private Assembly assembly;
        //已编译
        private bool IsCompiled;
        private string _ScriptCode;

        public string ScriptCode
        {
            get { return _ScriptCode; }
            set
            {
                SetProperty(ref _ScriptCode, value);
                IsCompiled = false;
                if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(FileName))
                    File.WriteAllText(FileName, value);
            }
        }

        private DelegateCommand _Run;

        public DelegateCommand Run =>
             _Run ??= new DelegateCommand(ExecuteRun);

        private async void ExecuteRun()
        {
            if (IsCompiled) goto run;

            try
            {
                ShowError("编译中，请稍等。");
                sharpBuilder = new AssemblyCSharpBuilder();
                //给编译器指定一个随机域
                sharpBuilder.Compiler.Domain = DomainManagement.Random;

                //使用文件编译模式，动态的程序集将编译进入DLL文件中
                // sharpBuilder.UseFileCompile();

                // 也可以使用内存流模式。
                sharpBuilder.UseStreamCompile();
                //如果代码编译错误，那么抛出并且记录日志。
                sharpBuilder.ThrowAndLogCompilerError();
                //如果语法检测时出错，那么抛出并记录日志，该步骤在编译之前。
                sharpBuilder.ThrowAndLogSyntaxError();
                //
                sharpBuilder.Add(ScriptCode);

                //编译得到程序集
                await Task.Run(() => assembly = sharpBuilder.GetAssembly());
                IsCompiled = true;
            }
            catch (Exception e)
            {
                ShowError("编译失败");
                ShowError(e.Message);
                return;
            }

        run:
            try
            {
                //在指定域创建一个 返回图像集 的Func 委托， 把刚才的程序集扔进去，
                var func = NDelegate.UseDomain(sharpBuilder.Compiler.Domain)

                    .Func<Mat, Dictionary<string, Mat>>(@"
                    return NatashaScript.Run(arg);
                    ");

                var mat = new Mat();
                if (Pool.SelectImage.HasValue)
                    mat = Pool.SelectImage.Value.Value;

                //调用委托 返回图像集
                if (mat.Empty())
                {
                    mat = new Mat(new Size(400, 400), MatType.CV_8UC3, Scalar.Black);
                    Cv2.Circle(mat, new Point(200, 200), 100, Scalar.Red, 5);
                }

                var listmat = func.Invoke(mat);

                //把图像集加入池
                foreach (var item in listmat)
                {
                    Pool.Images[item.Key] = item.Value;
                }
                ShowError("运行结束");
            }
            catch (Exception e)
            {
                ShowError("运行失败");
                ShowError(e.Message);
                return;
            }
        }


        private DelegateCommand _Load;
        public DelegateCommand Load =>
            _Load ??= new DelegateCommand(ExecuteLoad);

        void ExecuteLoad()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".*",
                Filter = "脚本文件(*.cscript)|*.cscript;*.txt;"
            };
            if (ofd.ShowDialog() == true)
            {
                FileName = ofd.FileName;

                ScriptCode = File.ReadAllText(FileName);

            }
        }
        private ObservableDictionary<string, string> _Files = new();

        public ObservableDictionary<string, string> Files
        {
            get { return _Files; }
            set { SetProperty(ref _Files, value); }
        }
        // 选择的文件
        private KeyValuePair<string, string>? _SelectFile;

        public KeyValuePair<string, string>? SelectFile
        {
            get { return _SelectFile; }
            set
            {
                SetProperty(ref _SelectFile, value);
                ScriptCode = File.ReadAllText(value.Value.Value);
                FileName = @"./Scripts/run/run.cscript";
            }
        }
        private string _FileName;
        public string FileName
        {
            get { return _FileName; }
            set { SetProperty(ref _FileName, value); }
        }


        private DelegateCommand _Save;
        public DelegateCommand Save =>
            _Save ??= new DelegateCommand(ExecuteSave);

        void ExecuteSave()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "脚本文件(*.cscript)|*.cscript;",
                Title = "Save File"
            };
            if (saveFileDialog.ShowDialog() == false) return;
            if (string.IsNullOrEmpty(saveFileDialog.FileName)) return;
            FileName = saveFileDialog.FileName;
            // if (!string.IsNullOrEmpty(value))
            //    File.WriteAllText(@"./Scripts/run.script", value);
            if (!string.IsNullOrEmpty(ScriptCode))
                File.WriteAllText(FileName, ScriptCode);

        }
        #endregion Natasha

        private readonly IEventAggregator _ea;

        private void ShowError(string errMsg)
        {
            _ea.GetEvent<MessageEvent>().Publish(new()
            {
                Target = "errLog",
                Content = errMsg
            });
        }
    }
}