using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace Toolbox.HierarchySearch
{
    [Icon(HierarchySearchOverlay.Icon)]
    public static class HierarchySearchTool
    {
        private static SearchListProvider _provider;

        private static GameObject[] GetObjectsinHierarchy()
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                var gameObjects = new List<GameObject>();
                GameObject rootPrefabObject = prefabStage.prefabContentsRoot;
                for (int i = 0; i < rootPrefabObject.transform.childCount; i++)
                {
                    gameObjects.Add(rootPrefabObject.transform.GetChild(i).gameObject);
                }

                return gameObjects.ToArray();
            }

            return Object.FindObjectsOfType<GameObject>();
        }

        [Shortcut("Tools/Hierarchy Search", KeyCode.F, ShortcutModifiers.Alt)]
        public static void Open()
        {
            if (_provider == null)
            {
                _provider = ScriptableObject.CreateInstance<SearchListProvider>();
            }

            _provider.Construct(GetObjectsinHierarchy());

            SearchWindow.Open(
                new SearchWindowContext(
                    GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                    _provider);
        }
    }

    [Overlay(typeof(SceneView), "Hierarchy Search")]
    public class HierarchySearchOverlay : ToolbarOverlay
    {
        public const string Icon = "Packages/com.game5mobile.hierarchysearch/Editor Default Resources/Magnifier@2x.png";

        HierarchySearchOverlay() : base(HierarchySearchButton.Id) { }

        [EditorToolbarElement(Id, typeof(SceneView))]
        class HierarchySearchButton : EditorToolbarButton, IAccessContainerWindow
        {
            public const string Id = "HierarchySearchOverlay/HierarchySearchButton";

            public EditorWindow containerWindow { get; set; }

            HierarchySearchButton()
            {
                text = "Search";
                tooltip = "Search in hierarchy (Alt + F)";
                icon = AssetDatabase.LoadAssetAtPath<Texture2D>(Icon);

                clicked += OnClick;
            }

            private void OnClick()
            {
                HierarchySearchTool.Open();
            }
        }
    }

    public class SearchListProvider : ScriptableObject, ISearchWindowProvider
    {
        private GameObject[] _gobjects;
        private Texture2D _icon;

        public void Construct(GameObject[] objs)
        {
            _gobjects = objs;
            _icon = new Texture2D(1, 1);
            _icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            _icon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchList = new();
            searchList.Add(new SearchTreeGroupEntry(new GUIContent("Hierarchy")));

            foreach (var o in _gobjects)
            {
                SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(o.name, _icon));
                entry.level = 1;
                entry.userData = o;
                searchList.Add(entry);
            }

            return searchList;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var selectedGameObject = SearchTreeEntry.userData as GameObject;
            Selection.activeGameObject = selectedGameObject;
            return true;
        }
    }
}
