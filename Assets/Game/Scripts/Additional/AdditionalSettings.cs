using TMPro;
using System;
using System.IO;
using UnityEngine;
using System.Linq;
using Scripts.Base.Helpers;
using System.Collections.Generic;

namespace Game.Scripts.Additional
{
    public class AdditionalSettings : MonoBehaviour
    {
        [SerializeField] private Game_Controller game_controller;
        [SerializeField] private FileManager fileManager;
        [SerializeField] private TMP_Text selectedImgName;
        [SerializeField] private GameObject cpasPanel;
        [SerializeField] private Transform cpasPlace;
        [SerializeField] private PuzzleInfo puzzleInfo;
        [SerializeField] private ASElement element;
        [SerializeField] private GameObject addUserImgBtn;
        private readonly string userDefaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private List<ASElement> loadedElements = new ();
        private string puzzleGamePath;
        private ASElement curElement;
        private bool isLoaded;
        
        [HideInInspector] public List<Texture2D> imgInfo = new();
        [HideInInspector] public string userFilesPath;
        
        private void Awake()
        {
            var firstInfo = puzzleInfo.data[0];
            selectedImgName.text = $"Выбрана категория:\n{firstInfo.namePuzzle.ToLower()}";
            imgInfo.AddRange(firstInfo.imagesPuzzle);
            
            CheckDirectory();
        }
        
        private void LoadElements()
        {
            bool isFirst = true;
            foreach (var info in puzzleInfo.data)
            {
                ASElement el = Instantiate(element, cpasPlace);

                if (isFirst)
                {
                    curElement = el;
                    el.outline.SetActive(true);
                    isFirst = false;
                }
                
                el.name.text = info.namePuzzle;
                el.SetImgInfo(info.imagesPuzzle);
                el.isUserImg = false;
                el.clickElement = SelectImg;
                el.StartSlideShow();
                loadedElements.Add(el);
            }
        }

        private void LoadUserElements()
        {
            List<string> allFolders = Directory.GetDirectories(userFilesPath).ToList();
            if (allFolders.Count <= 0)
                return;
            
            foreach (string folder in allFolders)
            {
                string[] allFiles = Directory.GetFiles(Path.Combine(userFilesPath, folder));
                List<Texture2D> imgUserInfo = (from file in allFiles where fileManager.IsImageFile(file, fileManager.imageExtensions) select LoadUserImage(file)).ToList();
                
                AddNewUserElement(Path.GetFileName(folder), imgUserInfo, folder);
            }
        }

        private void CheckDirectory()
        {
            puzzleGamePath = Path.Combine(userDefaultPath, "PuzzleGame");
            userFilesPath = Path.Combine(puzzleGamePath, "UserFiles");

            CheckPath(puzzleGamePath);
            CheckPath(userFilesPath);
        }

        private void CheckPath(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private void SelectImg(ASElement selectedElement)
        {
            if (curElement != null)
                curElement.ChangeSelected(false);
            
            curElement = selectedElement;
            curElement.ChangeSelected(true);
            imgInfo = curElement.GetImgInfo();
            selectedImgName.text = $"Выбрана категория:\n{curElement.name.text.ToLower()}";
        }
        
        private void SelectDefaultImg()
        {
            SelectImg(loadedElements[0]);
        }
        
        private void LoadAllElements()
        {
            isLoaded = true;
            LoadElements();
            LoadUserElements();
            addUserImgBtn.transform.SetAsLastSibling();
        }
        
        public void SetPictureInfo()
        {
            game_controller.SetPictureInfo(imgInfo);
        }
        
        public Texture2D LoadUserImage(string path)
        {
            if (path == null)
                return null;

            byte[] data = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(64, 64, TextureFormat.ARGB32, false);
            texture.LoadImage(data);
            texture.name = Path.GetFileNameWithoutExtension(path);
            var img = texture;

            return img;
        }
        
        public void ConfirmSelection()
        {
            SelectImg(curElement);
            cpasPanel.SetActive(false);
        }
        
        public void ChangeImg()
        {
            cpasPanel.SetActive(true);
            
            if (!isLoaded)
                LoadAllElements();
            else if (loadedElements.Count > 0)
                foreach (var loadedElement in loadedElements)
                    loadedElement.StartSlideShow();
        }

        public void AddNewUserElement(string cityName, List<Texture2D> info, string folderPath)
        {
            ASElement el = Instantiate(element, cpasPlace);
            el.name.text = cityName;
            el.SetImgInfo(info);
            el.isUserImg = true;
            el.deleteElement = SelectDefaultImg;
            el.clickElement = SelectImg;
            el.pathToFolder = folderPath;
            el.clickEditElement = fileManager.OpenEditKitPanel;
            el.StartSlideShow();
            loadedElements.Add(el);
            addUserImgBtn.transform.SetAsLastSibling();
        }
    }
}