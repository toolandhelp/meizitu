using Serilog;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace meizitu
{
    class Program
    {
        static string basePath = Directory.GetCurrentDirectory();
        static void Main(string[] args)
        {
            //日志
            Log.Logger = new LoggerConfiguration()
                  .MinimumLevel.Debug()
                  .WriteTo.Console()
                  .CreateLogger();

            Log.Information("开始日志操作：");

            Log.Information("文件将会保存到：" + basePath);

            #region 获取用户输入的年份
            Log.Warning("   ");
            Log.Warning("请输入年份：2012 <--> 2018 ： 回车开启！");

            int year = 2018; //多少年
            try
            {
                year = int.Parse(Console.ReadLine());
            }
            catch (Exception)
            {

                Log.Debug("检测到输入的不是年份数字，程序将获取2018年数据 ，请按任意键进行下一步");

                Console.ReadKey();
            }


            if (year < 2012)
            {
                Log.Debug("输入年份不正确，不能大于2012，请重新运行程序");

                Console.ReadKey();
            }
            if (year > 2018)
            {
                Log.Debug("输入年份不正确，不能大于2018，请重新运行程序");

                Console.ReadKey();
            }

            #endregion

            #region 字段拼接


            int proId = 1; //第几个项目
            int proImgIndex = 1;//第多少张图片
            string reqUrl = "https://images.weserv.nl/?url=pic.topmeizi.com/wp-content/uploads/"; //请求路径

            //月循环
            for (int i = 1; i <= 12; i++)
            {
                Log.Information("{0}年份,第{1}个月,开始", year, i);

                //是否改变月，如果到2就是需要改成下一个月了
                int isChangMonth = 0;

                // month = i;
                //第几个项目
                for (int k = 1; k <= proId; k++)
                {
                    Log.Information("{0}年份,第{1}个月,第{2}个项目", year, i, proId);

                    string fullBasePath = basePath + "\\" + year + i.ToString("00") + "\\" + proId.ToString("00");
                    if (Directory.Exists(fullBasePath))
                    {
                        Log.Information("此文件夹,图片可能已经有啦！");
                        Log.Information("请手动删除，将会重新下载");
                        proId++;
                    }
                    else
                    {
                        Log.Information("创建文件夹路径");
                        Directory.CreateDirectory(fullBasePath);

                        //reqUrl = string.Format("https://images.weserv.nl/?url=pic.topmeizi.com/wp-content/uploads/{0}a/{1}/{2}/{3}.jpg", year, i.ToString("00"), proId.ToString("00"), proImgIndex);
                        //第多少张图片
                        for (int j = 1; j <= proImgIndex; j++)
                        {
                            Thread.Sleep(5000);
                            Log.Information("{0}年份,第{1}个月,第{2}个项目,等{3}张图片", year, i, proId, proImgIndex);
                            string img = string.Format("{0}a/{1}/{2}/{3}.jpg", year, i.ToString("00"), proId.ToString("00"), proImgIndex.ToString("00"));

                            Log.Information("文件请求路径:" + img);
                            string pathName = Path.Combine(fullBasePath, proImgIndex.ToString("00") + ".jpg");

                            //获取下一个下项目
                            if (Download(reqUrl + img, pathName))
                            {
                                proImgIndex++;
                                isChangMonth = 0;
                            }
                            else
                            {
                                proImgIndex = 1;
                                isChangMonth++;
                                Log.Information("进行下一个项目获取");
                                break;
                            }

                        }

                        if (isChangMonth < 2)
                        {
                            proId++;
                        }
                        else
                        {
                            proId = 1;
                            Directory.Delete(fullBasePath);
                            Log.Information("进行下一个月项目获取");
                            break;
                        }

                    }
                }
                Log.Information("开始换下一个月份");
                //更换月份

            }





            #endregion

            Log.Information("    ");
            Log.Warning("已经结束！请按任意键退出");

            Console.ReadKey();
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pathName"></param>
        /// <param name="isNext"></param>
        /// <returns></returns>
        public static bool Download(string url, string pathName)
        {
            WebRequest request = WebRequest.Create(url);
            Log.Information("创建请求报文");
            try
            {
                using (WebResponse response = (WebResponse)request.GetResponse())
                {
                    Log.Information("响应回来");
                    using (Stream stream = response.GetResponseStream())//原始
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            Log.Information("保存下载图片");
                            #region 保存下载图片
                            Byte[] buffer = new Byte[response.ContentLength];
                            int offset = 0, actuallyRead = 0;
                            do
                            {
                                actuallyRead = stream.Read(buffer, offset, buffer.Length - offset);
                                offset += actuallyRead;
                            }
                            while (actuallyRead > 0);
                            using (MemoryStream ms = new MemoryStream(buffer))
                            {
                                byte[] buffurPic = ms.ToArray();
                                System.IO.File.WriteAllBytes(pathName, buffurPic);
                            }

                            Log.Information("保存下载图片完成=》路径：" + pathName);
                            #endregion
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error("此项目已经完结");
                return false;
            }
            return true;
        }

    }
}
