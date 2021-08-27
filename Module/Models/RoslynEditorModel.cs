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
            if (!File.Exists(@"./Scripts/run.script"))
                File.Copy(@"./Scripts/new.script", @"./Scripts/run.script");
            ScriptCode = File.ReadAllText(@"./Scripts/run.script");
        }

        private AssemblyCSharpBuilder sharpBuilder;
        private Assembly assembly;
        private bool IsCompiled;
        private string _ScriptCode;

        public string ScriptCode
        {
            get { return _ScriptCode; }
            set
            {
                SetProperty(ref _ScriptCode, value);
                IsCompiled = false;
                if (!string.IsNullOrEmpty(value))
                    File.WriteAllText(@"./Scripts/run.script", value);
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

                //调用委托 返回图像集
                if (!Pool.SelectImage.HasValue)
                {
                    ShowError("源图像不能为空！");
                    return;
                }
                var listmat = func.Invoke(Pool.SelectImage.Value.Value);

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