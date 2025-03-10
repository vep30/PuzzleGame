using TMPro;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.Collections;
using Game.Scripts.Additional;
using System.Collections.Generic;

namespace Scripts.Base.Helpers
{
    public class FileManager : MonoBehaviour
    {
        [SerializeField] private AdditionalSettings additionalSettings;
        [SerializeField] private TMP_InputField inputName;
        [SerializeField] private UserImg userImgElement;
        [SerializeField] private Transform userImgPlace;
        [SerializeField] private GameObject addUserImgBtn;
        [SerializeField] private float hideErrorTime;
        [SerializeField] private Image errorName, errorImgs;
        
        public string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        
        private bool CheckLUIP(string p) => loadUserImgsPaths.Contains(p); // LUIP - LoadUserImgsPaths
        private bool CheckFN(string fn) => fileNames.Contains(fn); // FN - FileNames
        
        private string filePath, fileName, folderPath;
        private ASElement element;
        private bool isEdit;
        private List<UserImg> userImgs;
        private List<Texture2D> imgInfo;
        private List<string> fileNames, userImgsPaths, loadUserImgsPaths, deleteImgsPaths;

        private void Awake()
        {
            ResetObjs();
        }

        private void ResetObjs()
        {
            loadUserImgsPaths = new();
            deleteImgsPaths = new ();
            userImgsPaths = new ();
            fileNames = new ();
            userImgs = new ();
            imgInfo = new();
            DisableErrorLight();
        }

        private void Start()
        {
            FileBrowser.SetFilters(false, new FileBrowser.Filter("Изображения", ".jpg", ".png"));
            FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");
        }

        private IEnumerator ShowLoadDialogCoroutine()
        {
            yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null,
                "Выбор изображения",
                "Загрузить");

            if (FileBrowser.Success)
                OnFilesSelected(FileBrowser.Result[0]);
        }

        private void OnFilesSelected(string filePaths)
        {
            string tempFileName = FileBrowserHelpers.GetFilename(filePaths);
            if (CheckLUIP(filePaths) || CheckFN(tempFileName))
                return;
            
            filePath = filePaths;
            loadUserImgsPaths.Add(filePath);
            CreateElement(filePath);
            addUserImgBtn.transform.SetAsLastSibling();
        }

        private void CreateElement(string pathFile)
        {
            string tempFileName = FileBrowserHelpers.GetFilename(pathFile);
            if (!string.IsNullOrEmpty(fileName))
                if (fileNames.Contains(tempFileName))
                    if (ReturnFromCreate())
                        return;
            
            if (CheckFN(tempFileName))
                return;
            
            fileName = tempFileName;
            fileNames.Add(fileName);
            byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(pathFile);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);
            Sprite sprite = texture.Texture2DToSprite();
            UserImg el = Instantiate(userImgElement, userImgPlace);
            el.elementDelete = DeleteElement;
            el.imgName = fileName;
            el.SetUserImg(sprite);
            userImgs.Add(el);
        }

        private bool ReturnFromCreate()
        {
            foreach (var deletePath in deleteImgsPaths.Where(deletePath => Path.GetFileName(deletePath) == fileName))
            {
                loadUserImgsPaths.Remove(deletePath);
                fileNames.Remove(fileName);
                return false;
            }

            return true;
        }
        
        private void DeleteElement(UserImg el)
        {
            if (string.IsNullOrEmpty(folderPath))
                folderPath = additionalSettings.userFilesPath;

            string deleteImgPath = Path.Combine(folderPath, el.imgName);
            if (File.Exists(deleteImgPath))
                deleteImgsPaths.Add(deleteImgPath);
            else if (Path.GetFileName(filePath) == el.imgName && CheckLUIP(filePath))
            {
                loadUserImgsPaths.Remove(filePath);
                if (CheckFN(fileName))
                    fileNames.Remove(fileName);
                fileName = filePath = "";
            }
            
            userImgs.Remove(el);
        }
        
        private void CopyFiles(List<string> listPaths)
        {
            foreach (var onePath in listPaths)
            {
                string destinationPath = Path.Combine(folderPath, Path.GetFileName(onePath));
                if (!File.Exists(destinationPath))
                    FileBrowserHelpers.CopyFile(onePath, destinationPath);
            }
        }

        private void SetImgInfo()
        {
            foreach (var userImgPath in loadUserImgsPaths)
                imgInfo.Add(additionalSettings.LoadUserImage(userImgPath));
        }

        private void DisableErrorLight()
        {
            errorName.gameObject.SetActive(false);
            errorImgs.gameObject.SetActive(false);
        }
        
        private IEnumerator HideErrorLightCor()
        {
            float curTime = 0;
            while (curTime < hideErrorTime)
            {
                curTime += Time.deltaTime;
                yield return null;
            }
            
            DisableErrorLight();
        }
        
        public void Accept()
        {
            string cityName = inputName.text;
            bool notName = string.IsNullOrEmpty(cityName);
            bool notImgs = userImgs.Count <= 0;
            errorName.gameObject.SetActive(notName);
            errorImgs.gameObject.SetActive(notImgs);

            if (notName || notImgs)
            {
                StartCoroutine(HideErrorLightCor());
                return;
            }

            folderPath = Path.Combine(additionalSettings.userFilesPath, cityName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            
            if (deleteImgsPaths.Count > 0)
            {
                foreach (var deleteImgPath in deleteImgsPaths)
                {
                    if (userImgsPaths.Contains(deleteImgPath))
                        userImgsPaths.Remove(deleteImgPath);
                    if (CheckLUIP(deleteImgPath))
                        loadUserImgsPaths.Remove(deleteImgPath);
                    string deleteFileName = Path.GetFileName(deleteImgPath);
                    string fileNameOnly = Path.GetFileNameWithoutExtension(deleteImgPath);
                    if (CheckFN(deleteFileName))
                        fileNames.Remove(deleteFileName);
                    
                    string filePathTxt = Path.Combine(folderPath, $"{fileNameOnly}.txt");
                    if (File.Exists(deleteImgPath))
                        File.Delete(deleteImgPath);
                    if (File.Exists(filePathTxt))
                        File.Delete(filePathTxt);
                }
            }
            
            CopyFiles(loadUserImgsPaths);
            CopyFiles(userImgsPaths);

            if (!isEdit)
            {
                SetImgInfo();
                additionalSettings.AddNewUserElement(cityName, imgInfo, folderPath);
            }
            else
            {
                userImgsPaths.AddRange(loadUserImgsPaths);
                loadUserImgsPaths = userImgsPaths;
                SetImgInfo();
                
                element.SetImgInfo(imgInfo);
                element.StartSlideShow();
            }
            
            HidePanel();
        }
        
        public bool IsImageFile(string pathToFile, string[] extensions)
        {
            string fileExtension = Path.GetExtension(pathToFile).ToLower();
            return extensions.Any(ext => fileExtension == ext);
        }

        public void ShowLoadDialog()
        {
            StartCoroutine(ShowLoadDialogCoroutine());
        }

        public void OpenEditKitPanel(ASElement el)
        {
            isEdit = true;
            element = el;
            gameObject.SetActive(true);
            string nameEl = element.name.text;
            inputName.text = nameEl;
            folderPath = Path.Combine(additionalSettings.userFilesPath, nameEl);
            
            string[] allFiles = Directory.GetFiles(folderPath);

            List<string> allImages = new List<string>();
            foreach (string file in allFiles)
                if (IsImageFile(file, imageExtensions))
                    allImages.Add(file);
            
            if (allImages.Count <= 0)
                return;
            
            userImgsPaths = allImages.ToList();
            foreach (var file in allImages)
                CreateElement(file);

            addUserImgBtn.transform.SetAsLastSibling();
        }

        public void HidePanel()
        {
            isEdit = false;
            gameObject.SetActive(false);
            inputName.SetTextWithoutNotify("");
            foreach (var userImg in userImgs)
                Destroy(userImg.gameObject);
            
            imgInfo.Clear();
            userImgs.Clear();
            fileNames.Clear();
            userImgsPaths.Clear();
            deleteImgsPaths.Clear();
            loadUserImgsPaths.Clear();
            filePath = fileName = folderPath = "";

            ResetObjs();
        }
    }
}