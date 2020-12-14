using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace OhMyFramework.Editor
{
    public class EditorCoroutine {
        public static EditorCoroutine start(IEnumerator _routine) {
            EditorCoroutine coroutine = new EditorCoroutine(_routine);
            coroutine.start();
            return coroutine;
        }

        readonly IEnumerator routine;
        EditorCoroutine(IEnumerator _routine) {
            routine = _routine;
        }

        public void start() {
            EditorApplication.update += update;
            isPlaying = true;
        }
        public void stop() {
            EditorApplication.update -= update;
            isPlaying = false;
        }

        bool isPlaying = false;
        public bool IsPlaying() {
            return isPlaying;
        }

        void update() {
            if (!routine.MoveNext()) {
                stop();
            }
        }
    }

    public interface IModuleEditor {
        void OnGUI();
        bool Initialize();
        void OnFocus();
        void OnLostFoce();
    }

    public abstract class ModuleEditor<T> : IModuleEditor where T : MonoBehaviour {

        public abstract void OnGUI();
        public abstract bool Initialize();

        public static void Repaint() {
            OhMyFrameworkPanel.RepaintAll();
        }

        public abstract void OnLostFoce();

        public virtual void OnFocus() { }
    }

// public abstract class MetaEditor : MetaEditor<MonoBehaviour> {}

// public class BerryPanelDefaultAttribute : Attribute {}

    public class OhMyFrameworkPanelTabAttribute : Attribute {
        float priority;
        string title;
        Texture2D icon = null;

        public OhMyFrameworkPanelTabAttribute(string title, int priority = 0) {
            this.title = title;
            this.priority = priority;
        }

        public OhMyFrameworkPanelTabAttribute(string title, string icon, int priority = 0) {
            this.title = title;
            this.priority = priority;
            if (!string.IsNullOrEmpty(icon)) {
                this.icon = EditorIcons.GetIcon(icon);
                if (!this.icon)
                    this.icon = EditorIcons.GetIcon(icon);
            }
        }

        public string Title {
            get {
                return title;
            }
        }

        public float Priority {
            get {
                return priority;
            }
        }

        public Texture2D Icon {
            get {
                return icon;
            }
        }
    }

    public class OhMyFrameworkPanelGroupAttribute : Attribute {
        string group;

        public OhMyFrameworkPanelGroupAttribute(string group) {
            this.group = group;
        }

        public string Group {
            get {
                return group;
            }
        }
    }

    public class PrefVariable {
        string key = "";
        public PrefVariable(string _key) {
            key = _key;
        }

        public int Int {
            get {
                return EditorPrefs.GetInt(key);
            }
            set {
                EditorPrefs.SetInt(key, value);
            }
        }

        public float Float {
            get {
                return EditorPrefs.GetFloat(key);
            }
            set {
                EditorPrefs.SetFloat(key, value);
            }
        }

        public string String {
            get {
                return EditorPrefs.GetString(key);
            }
            set {
                EditorPrefs.SetString(key, value);
            }
        }

        public bool Bool {
            get {
                return EditorPrefs.GetBool(key);
            }
            set {
                EditorPrefs.SetBool(key, value);
            }
        }

        public bool IsEmpty() {
            return !EditorPrefs.HasKey(key);
        }

        public void Delete() {
            EditorPrefs.DeleteKey(key);
        }
    }

    public static class EUtils {
        /**
         * 
         * 1.drawSingle,返回一个目标值，通过setValue设置给目标
         * 2.drawMixed，通过setDefault把值传给目标，返回true、false标记是否重新赋值
         * 
         */
        public static void DrawMixedProperty<T>(IEnumerable<int2> selected, Func<int2, bool> mask, Func<int2, T> getValue, Action<int2, T> setValue, Func<int2, T, T> drawSingle, Func<Action<T>, bool> drawMixed, Action drawEmpty = null) {
            bool multiple = false;
            bool assigned = false;
            T value = default(T);
            T temp;
            int2 last_coord = null;
            foreach (int2 coord in selected) {
                if (!mask.Invoke(coord))
                    continue;
                if (!assigned) {
                    value = getValue.Invoke(coord);
                    last_coord = coord;
                    assigned = true;
                    continue;
                }
                last_coord = null;
                temp = getValue.Invoke(coord);
                if (!value.Equals(temp)) {
                    multiple = true;
                    break;
                }
            }

            if (!assigned) {
                if (drawEmpty != null)
                    drawEmpty.Invoke();
                return;
            }

            if (multiple) {
                EditorGUI.showMixedValue = true;
                Action<T> setDefault = (T t) => {
                    value = t;
                };
                if (drawMixed.Invoke(setDefault)) {
                    multiple = false;
                }
                EditorGUI.showMixedValue = false;
            } else
                value = drawSingle(last_coord, value);

            if (!multiple)
                foreach (int2 coord in selected)
                    if (mask.Invoke(coord))
                        setValue(coord, value);
        }

        public static List<FileInfo> SearchFiles(string directory) {
            List<FileInfo> result = new List<FileInfo>();
            result.AddRange(new DirectoryInfo(directory).GetFiles().ToList());
            foreach (DirectoryInfo dir in new DirectoryInfo(directory).GetDirectories())
                result.AddRange(SearchFiles(dir.FullName));
            return result;
        }

        public static string BytesToString(long byteCount) {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        public static IEnumerable<FileInfo> ProjectFiles(DirectoryInfo directory) {
            foreach (FileInfo file in directory.GetFiles())
                yield return file;
            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            foreach (FileInfo file in ProjectFiles(subDirectory))        
                yield return file;
        }
        public static IEnumerable<FileInfo> ProjectFiles(string directory) {
            return ProjectFiles(new DirectoryInfo(directory));
        }

        public static int2 GetSize(string sizeStr)
        {
            int2 size;

            if (!string.IsNullOrEmpty(sizeStr))
            {
                string[] sizeInt = sizeStr.Split('x');
                if (sizeInt.Length == 2)
                {
                    size = new int2(int.Parse(sizeInt[0]), int.Parse(sizeInt[1]));
                    return size;
                }
            }
            size = new int2(1, 1);
            return size;

        }


    }

    public static class GUIHelper {
        public class Vertical : IDisposable {
            public Vertical(params GUILayoutOption[] options) {
                EditorGUILayout.BeginVertical(options);
            }

            public Vertical(out Rect rect, params GUILayoutOption[] options) {
                rect = EditorGUILayout.BeginVertical(options);
            }

            public Vertical(GUIStyle style, params GUILayoutOption[] options) {
                EditorGUILayout.BeginVertical(style, options);
            }

            public Vertical(out Rect rect, GUIStyle style, params GUILayoutOption[] options) {
                rect = EditorGUILayout.BeginVertical(style, options);
            }

            public void Dispose() {
                EditorGUILayout.EndVertical();
            }
        }

        public class Horizontal : IDisposable {
            public Horizontal(params GUILayoutOption[] options) {
                EditorGUILayout.BeginHorizontal(options);
            }

            public Horizontal(out Rect rect, params GUILayoutOption[] options) {
                rect = EditorGUILayout.BeginHorizontal(options);
            }

            public Horizontal(GUIStyle style, params GUILayoutOption[] options) {
                EditorGUILayout.BeginHorizontal(style, options);
            }

            public Horizontal(out Rect rect, GUIStyle style, params GUILayoutOption[] options) {
                rect = EditorGUILayout.BeginHorizontal(style, options);
            }

            public void Dispose() {
                EditorGUILayout.EndHorizontal();
            }
        }

        public class Scroll {
            Vector2 position = new Vector2();
            GUILayoutOption[] options = null;
            GUIStyle style = null;

            public Scroll(GUIStyle style, params GUILayoutOption[] options) : this(options) {
                this.style = style;
            }

            public Scroll(params GUILayoutOption[] options) {
                this.options = options;
            }

            public ScrollDisposable Start() {
                return new ScrollDisposable(ref position, style, options);
            }

            public void ScrollTo(float y = 0) {
                position.y = y;
            }

            public class ScrollDisposable : IDisposable {

                public ScrollDisposable(ref Vector2 position, GUIStyle style, params GUILayoutOption[] options) {
                    if (style == null)
                        position = EditorGUILayout.BeginScrollView(position, options);
                    else
                        position = EditorGUILayout.BeginScrollView(position, style, options);
                }

                public void Dispose() {
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        public class Lock : IDisposable {
            bool memory;
            public Lock(bool enabled) {
                memory = GUI.enabled;
                if (memory)
                    GUI.enabled = !enabled;
            }

            public void Dispose() {
                GUI.enabled = memory;
            }
        }

        public class Color : IDisposable {
            UnityEngine.Color memory;
            public Color(UnityEngine.Color color) {
                memory = GUI.color;
                GUI.color = color;
            }

            public void Dispose() {
                GUI.color = memory;
            }
        }

        public class ContentColor : IDisposable {
            UnityEngine.Color memory;
            public ContentColor(UnityEngine.Color color) {
                memory = GUI.contentColor;
                GUI.contentColor = color;
            }

            public void Dispose() {
                GUI.contentColor = memory;
            }
        }

        public class BackgroundColor : IDisposable {
            UnityEngine.Color memory;
            public BackgroundColor(UnityEngine.Color color) {
                memory = GUI.backgroundColor;
                GUI.backgroundColor = color;
            }

            public void Dispose() {
                GUI.backgroundColor = memory;
            }
        }

        public class Change : IDisposable {
            Action onChanged;
            public Change(Action onChanged) {
                EditorGUI.BeginChangeCheck();
                this.onChanged = onChanged;
            }

            public void Dispose() {
                if (EditorGUI.EndChangeCheck())
                    onChanged.Invoke();
            }
        }

        public class LayoutSplitter {
            int resize = -1;
            int current = 0;
            float[] sizes;

            Rect lastRect;

            public float thickness = 4;
            public Action<Rect> drawCursor = delegate {};

            OrientationLine orientation;
            OrientationLine internalOrientation;

            bool areaStarted = false;
            bool firstArea = false;
    
            public LayoutSplitter(OrientationLine orientation, OrientationLine internalOrientation, params float[] sizes) {
                this.sizes = sizes;
                this.orientation = orientation;
                this.internalOrientation = internalOrientation;
            }

            public bool Area(GUIStyle style = null) {
                if (IsLast() && !firstArea)
                    return false;

                if (mask != null && !mask[current]) {
                    current++;
                    return false;
                }

                if (areaStarted) {
                    areaStarted = false;
                    EndLayout(internalOrientation);
                }
                if (!firstArea) {
                    lastRect = GUILayoutUtility.GetLastRect();
                    Rect rect = EditorGUILayout.GetControlRect(Expand(Anti(orientation)), Size(orientation, thickness));
                    drawCursor(rect);

                    EditorGUIUtility.AddCursorRect(rect, orientation == OrientationLine.Horizontal ? MouseCursor.ResizeHorizontal : MouseCursor.ResizeVertical);

                    if (resize < 0 && Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                        resize = current;

                    if (resize == current) {
                        float delta = 0;
                        switch (orientation) {
                            case OrientationLine.Horizontal: delta = Mathf.Max(lastRect.width - (rect.x - Event.current.mousePosition.x + thickness / 2), 50); break;
                            case OrientationLine.Vertical: delta = Mathf.Max(lastRect.height - (rect.y - Event.current.mousePosition.y + thickness / 2), 50); break;
                        }

                        delta -= sizes[current];

                        sizes[current] += delta;

                        if (EditorWindow.mouseOverWindow)
                            EditorWindow.mouseOverWindow.Repaint();

                        if (Event.current.type == EventType.MouseUp)
                            resize = -1;
                    }
                }

                if (firstArea)
                    firstArea = false;
                else
                    current++;
        
                if (IsLast())
                    BeginLayout(internalOrientation, style);
                else
                    BeginLayout(internalOrientation, style, Size(orientation, sizes[current]));

                areaStarted = true;

                if (mask != null)
                    return mask[current];

                return true;
            }

            OrientationLine Anti(OrientationLine o) {
                switch (o) {
                    case OrientationLine.Horizontal: return OrientationLine.Vertical;
                    case OrientationLine.Vertical: return OrientationLine.Horizontal;
                }
                return OrientationLine.Horizontal;
            }

            Rect BeginLayout(OrientationLine o, GUIStyle style = null, params GUILayoutOption[] options) {
                switch (o) {
                    case OrientationLine.Horizontal: return style == null ? EditorGUILayout.BeginHorizontal(options) : EditorGUILayout.BeginHorizontal(style, options);
                    case OrientationLine.Vertical: return style == null ? EditorGUILayout.BeginVertical(options) : EditorGUILayout.BeginVertical(style, options);
                }
                return new Rect();
            }

            void EndLayout(OrientationLine o, params GUILayoutOption[] options) {
                switch (o) {
                    case OrientationLine.Horizontal: EditorGUILayout.EndHorizontal(); break;
                    case OrientationLine.Vertical: EditorGUILayout.EndVertical(); break;
                }
            }

            GUILayoutOption Size(OrientationLine o, float size) {
                switch (o) {
                    case OrientationLine.Horizontal: return GUILayout.Width(size);
                    case OrientationLine.Vertical: return GUILayout.Height(size);
                }
                return null;
            }

            GUILayoutOption Expand(OrientationLine o) {
                switch (o) {
                    case OrientationLine.Horizontal: return GUILayout.ExpandWidth(true);
                    case OrientationLine.Vertical: return GUILayout.ExpandHeight(true);
                }
                return null;
            }

            bool[] mask = null;
            public Splitter Start(params bool[] mask) {
                last = sizes.Length;
                UpdateMask(mask);

                current = 0;
                areaStarted = false;
                firstArea = true;

                BeginLayout(orientation, null, Expand(orientation));

                if (current >= sizes.Length)
                    return new Splitter(() => EndLayout(orientation));

                return new Splitter(() => {
                    if (areaStarted)
                        EndLayout(internalOrientation);
                    EndLayout(orientation);
                });
            }

            public void UpdateMask(params bool[] mask) {
                if (mask != null && mask.Length == sizes.Length + 1) {
                    this.mask = mask;
                    while (last >= 0 && !mask[last])
                        last--;
                } else
                    this.mask = null;
            }

            int last = -1;
            bool IsLast() {
                return current >= last;
            }

            public class Splitter : IDisposable {
                Action onDispose;

                public Splitter(Action onDispose) {
                    this.onDispose = onDispose;
                }

                public void Dispose() {
                    onDispose();
                }
            }
        }

        public static void DrawSprite(Sprite sprite) {
            DrawSprite(sprite, sprite.texture.width, sprite.texture.height);
        }

        public static void DrawSprite(Sprite sprite, float width, float height) {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(width), GUILayout.Height(height));
            if (sprite != null) {
                Rect uv = sprite.rect;
                uv.x /= sprite.texture.width;
                uv.width /= sprite.texture.width;
                uv.y /= sprite.texture.height;
                uv.height /= sprite.texture.height;
                GUI.DrawTextureWithTexCoords(rect, sprite.texture, uv);
            }
        }

        public abstract class HierarchyList<I, F> : TreeView {
            protected FolderInfo root;

            public List<I> itemCollection = null;
            public List<F> folderCollection = null;

            internal Dictionary<int, IInfo> info = new Dictionary<int, IInfo>();
            internal Dictionary<string, F> folders = new Dictionary<string, F>();

            public Action<List<I>> onSelectedItemChanged = delegate { };
            public Action<List<IInfo>> onSelectionChanged = delegate { };
            public Action onRebuild = delegate { };
            public Action onChanged = delegate { };
            public Action<List<IInfo>> onRemove = delegate { };

            protected string listName = null;
            public HierarchyList(List<I> collection, List<F> folders, TreeViewState state, string name = null) : base(state) {
                listName = name;

                this.itemCollection = collection;
                this.folderCollection = folders != null ? folders : new List<F>();
                Reload();
            }

            protected override TreeViewItem BuildRoot() {
                info.Clear();
                root = new FolderInfo(-1, null);

                folders = folderCollection.GroupBy(x => GetFullPath(x)).ToDictionary(x => x.Key, x => x.First());

                foreach (I element in itemCollection) {
                    FolderInfo folder = AddFolder(GetPath(element));
                    ItemInfo i = new ItemInfo(folder.item.depth + 1, folder);
                    i.content = element;
                    i.item.displayName = GetName(element);
                    folder.items.Add(i);
                }

                folderCollection.Clear();
                folderCollection.AddRange(folders.Values);
                foreach (F folder in folderCollection.ToList())
                    AddFolder(GetFullPath(folder));

                if (!string.IsNullOrEmpty(_searchFilter)) {
                    string filter = _searchFilter.ToLower();
                    bool byPath = _searchFilter.Contains('/');
                    FolderInfo filteredRoot = new FolderInfo(-1, null);
                    foreach (IInfo item in root.GetAllChild()) {
                        string name = byPath ? GetFullPath(item) : GetName(item);
                        if (name.ToLower().Contains(filter)) {
                            item.item.parent = filteredRoot.item;
                            item.item.depth = 0;
                            filteredRoot.items.Add(item);
                            if (item is FolderInfo) (item as FolderInfo).items.Clear();
                        }
                    }
                    root = filteredRoot;
                }
        
                root.GetAllChild().ForEach(x => x.item.id = GetUniqueID(x));

                info.Clear();
                foreach (IInfo iinfo in root.GetAllChild()) {
                    if (info.ContainsKey(iinfo.item.id))
                        throw new ArgumentException(string.Format("These two elements has the same ID ({0})\n{1}\n{2}", iinfo.item.id,
                            GetFullPath(iinfo),
                            GetFullPath(info[iinfo.item.id])));
                    info.Add(iinfo.item.id, iinfo);
                }
                SetupParentsAndChildrenFromDepths(root.item, root.GetAllChild().Select(x => x.item).ToList());

                onRebuild();
                SelectionChanged(state.selectedIDs);
                return root.item;
            }

            string _searchFilter = "";
            public string searchFilter {
                get {
                    return _searchFilter;
                }
            }
            public void SetSearchFilter(string filter) {
                if (_searchFilter != filter) {
                    _searchFilter = filter;
                    Reload();
                }
            }

            public IInfo GetInfo(I item) {
                return info.Values.FirstOrDefault(x => x.isItemKind && x.asItemKind.content.Equals(item));
            }

            protected override void SelectionChanged(IList<int> selectedIds) {
                if (onSelectedItemChanged.GetInvocationList().Length == 0)
                    return;
                List<I> result = new List<I>();
                foreach (int id in selectedIds) {
                    IInfo item = info.Get(id);
                    if (item != null && item.isItemKind)
                        result.Add(item.asItemKind.content);
                }
                onSelectedItemChanged(result);
                onSelectionChanged(selectedIds.Select(x => info.Get(x)).Where(x => x != null).ToList());
            }

            public abstract bool ObjectToItem(UnityEngine.Object o, out IInfo result);

            protected override bool CanStartDrag(CanStartDragArgs args) {
                return true;
            }

            protected override void RowGUI(RowGUIArgs args) {
                IInfo i = info.Get(args.item.id);
                if (i == null) return;
                Rect rect = new Rect(args.rowRect);
                float offset = (args.item.depth + 1) * 16;
                rect.x += offset;
                rect.width -= offset;
                if (i.isItemKind) DrawItem(rect, i.asItemKind);
                else DrawFolder(rect, i.asFolderKind);
            }

            protected override float GetCustomRowHeight(int row, TreeViewItem item) {
                return info[item.id].isItemKind ? ItemRowHeight() : FolderRowHeight();
            }

            protected override bool CanRename(TreeViewItem item) {
                IInfo iinfo = info.Get(item.id);
                if (iinfo == null) return false;
                return iinfo.isItemKind ? CanRename(iinfo.asItemKind) : CanRename(iinfo.asFolderKind);
            }
            public virtual bool CanRename(ItemInfo info) {
                return true;
            }
            public virtual bool CanRename(FolderInfo info) {
                return true;
            }
            protected override void RenameEnded(RenameEndedArgs args) {
                if (args.originalName == args.newName || args.newName.Contains('/')) return;
                IInfo iinfo = info[args.itemID];
                if (iinfo.parent.items.Contains(x => GetName(x) == args.newName)) return;

                SetName(iinfo, args.newName);
                iinfo.item.displayName = args.newName;
                UpdatePath(new List<IInfo>() { iinfo });
                Reload();
                onChanged();
            }



            public virtual float FolderRowHeight() {
                return 16f;
            }
            public virtual float ItemRowHeight() {
                return 16f;
            }

            public virtual bool CanBeChild(IInfo parent, IInfo child) {
                return parent.isFolderKind &&
                       (child.parent == parent || parent.asFolderKind.items.FirstOrDefault(x => GetName(x) == GetName(child)) == null);
            }

            public abstract void DrawItem(Rect rect, ItemInfo info);
            public abstract void DrawFolder(Rect rect, FolderInfo info);

            public abstract string GetPath(I element);
            public abstract string GetPath(F element);
            string GetFullPath(IInfo info) {
                if (info.isItemKind) return GetFullPath(info.asItemKind.content);
                else return GetFullPath(info.asFolderKind.content);
            }
            string GetFullPath(I element) {
                string path = GetPath(element);
                string name = GetName(element);
                if (path.Length > 0) return path + '/' + name;
                else return name;
            }
            string GetFullPath(F element) {
                string path = GetPath(element);
                string name = GetName(element);
                if (path.Length > 0) return path + '/' + name;
                else return name;
            }

            public void SetPath(IInfo info, string path) {
                if (info.isItemKind) SetPath(info.asItemKind.content, path);
                else SetPath(info.asFolderKind.content, path);
            }
            public abstract void SetPath(I element, string path);
            public abstract void SetPath(F element, string path);


            public void SetName(IInfo info, string name) {
                if (info.isItemKind) {
                    SetName(info.asItemKind.content, name);
                }else {
                    SetName(info.asFolderKind.content, name);
                }
            }
            public virtual void SetName(I element, string name) {}
            public virtual void SetName(F element, string name) {}

            public string GetName(IInfo info) {
                return info.isItemKind ? GetName(info.asItemKind.content) : GetName(info.asFolderKind.content);
            }
            public virtual string GetName(I element) {
                return "Item";
            }
            public virtual string GetName(F element) {
                string path = GetPath(element);
                int sep = path.IndexOf("/");
                if (sep >= 0) return path.Substring(sep + 1, path.Length - sep - 1);
                return path;
            }

            int GetUniqueID(IInfo element) {
                return element is ItemInfo ? GetUniqueID((element as ItemInfo).content) : GetUniqueID((element as FolderInfo).content);
            }
            public abstract int GetUniqueID(I element);
            public abstract int GetUniqueID(F element);
    
            protected FolderInfo AddFolder(string fullPath) {
                FolderInfo currentFolder = root;
                if (!string.IsNullOrEmpty(fullPath)) {
                    foreach (string name in fullPath.Split('/')) {
                        if (name == "") continue;
                        FolderInfo f = (FolderInfo) currentFolder.items.Find(x => x.isFolderKind && GetName(x.asFolderKind.content) == name);
                        if (f == null) {
                            f = new FolderInfo(currentFolder.item.depth + 1, currentFolder);
                            currentFolder.items.Add(f);
                            f.parent = currentFolder;
                            f.item.displayName = name;

                            string path = f.fullPath;
                            if (!folders.ContainsKey(path)) {
                                F folder = CreateFolder();
                                SetPath(folder, currentFolder.fullPath);
                                SetName(folder, name);
                                folderCollection.Add(folder);
                                folders.Add(path, folder);
                            }
                            f.content = folders[path];
                        }
                        currentFolder = f;
                    }
                }
                return currentFolder;
            }
            public void AddNewFolder(FolderInfo folder, string nameFormat) {
                if (nameFormat == null || !nameFormat.Contains("{0}"))
                    nameFormat = "Untitled{0}";

                if (folder == null) folder = root;

                string name = string.Format(nameFormat, "");
                for (int i = 1; true; i++) {
                    if (!folder.items.Contains(x => GetName(x) == name))
                        break;
                    name = string.Format(nameFormat, i);
                }

                string path = folder.fullPath;
                F newFolder = CreateFolder();
                SetName(newFolder, name);
                SetPath(newFolder, path);
                int id = GetUniqueID(newFolder);

                folderCollection.Add(newFolder);
                Reload();
                onChanged();

                var treeItem = FindItem(id, root.item);
                if (CanRename(treeItem))
                    BeginRename(treeItem);
            }
            public void Group(List<IInfo> items, FolderInfo parent, string nameFormat) {
                if (!nameFormat.Contains("{0}"))
                    nameFormat = "Untitled{0}";

                if (parent == null) parent = root;

                string name = string.Format(nameFormat, "");
                for (int i = 1; true; i++) {
                    if (!parent.items.Contains(x => GetName(x) == name))
                        break;
                    name = string.Format(nameFormat, i);
                }

                FolderInfo group = AddFolder(parent.fullPath + "/" + name);
                PutInFolder(group, parent, items.Min(x => x.index));
                int id = GetUniqueID(group);

                foreach (IInfo item in items)
                    PutInFolder(item, group);

                UpdatePath(items);
                Reload();
                onChanged();

                var treeItem = FindItem(id, root.item);
                if (CanRename(treeItem))
                    BeginRename(treeItem);
            }

            public abstract F CreateFolder();
        
            public void Remove(params IInfo[] items) {
                if (items == null || items.Length == 0)
                    return;
                if (!EditorUtility.DisplayDialog("Remove", "Are you sure want to remove these items", "Remove", "Cancel"))
                    return;

                List<IInfo> toRemove = new List<IInfo>();
                foreach (IInfo iinfo in items) {
                    toRemove.Add(iinfo);
                    if (iinfo.isFolderKind)
                        toRemove.AddRange(iinfo.asFolderKind.GetAllChild());
                }
                toRemove.Distinct();
                onRemove(toRemove);
                foreach (IInfo iinfo in toRemove) {
                    if (iinfo.isItemKind) itemCollection.Remove(iinfo.asItemKind.content);
                    if (iinfo.isFolderKind) folderCollection.Remove(iinfo.asFolderKind.content);
                    // removeRealFileOrFolder(iinfo);
                }
                MarkAsChanged();
            }

            bool reloadRequired = false;
            public void MarkAsChanged() {
                reloadRequired = true;
            }
        
            /// <summary>
            /// Removes the real file or folder.
            /// </summary>
            /// <param name="iinfo">Iinfo.</param>

            public override void OnGUI(Rect rect) {
                base.OnGUI(rect);
                if (reloadRequired) {
                    Reload();
                    onChanged();
                    reloadRequired = false;
                }
                
            }

            public void OnGUI(params GUILayoutOption[] options) {
                OnGUI(EditorGUILayout.GetControlRect(options));
            }

            public void OnGUI() {
                OnGUI(GUILayout.ExpandWidth(true), GUILayout.Height(Mathf.Max(rowHeight, totalHeight)));
            }

            public void AddNewItem(FolderInfo folder, string nameFormat) {
                if (nameFormat == null || !nameFormat.Contains("{0}"))
                    nameFormat = "Untitled{0}";
                
                if (folder == null) folder = root;

                string name = string.Format(nameFormat, "");
                for (int i = 1; true; i++) {
                    if (!folder.items.Contains(x => GetName(x) == name))
                        break;
                    name = string.Format(nameFormat, i);
                }

                string path = folder.fullPath;
                I newItem = CreateItem();
                SetName(newItem, name);
                SetPath(newItem, path);
                int id = GetUniqueID(newItem);

                itemCollection.Add(newItem);
                Reload();
                onChanged();

                var treeItem = FindItem(id, root.item);
                if (CanRename(treeItem))
                    BeginRename(treeItem);
            }
            public abstract I CreateItem();
    
            List<IInfo> drag = new List<IInfo>();
            protected override void SetupDragAndDrop(SetupDragAndDropArgs args) {
                DragAndDrop.PrepareStartDrag();

                drag.Clear();
                foreach (var id in args.draggedItemIDs)
                    drag.Add(root.Find(id));

                DragAndDrop.paths = null;
                DragAndDrop.objectReferences = new UnityEngine.Object[0];
                DragAndDrop.SetGenericData(typeof(I).Name, drag.Where(x => x.isItemKind).Select(x => x.asItemKind.content).ToList());
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                DragAndDrop.StartDrag("HierarchyList");
                _isDrag = true;
            }

            bool _isDrag = false;
            public bool isDrag {
                get {
                    return _isDrag;
                }
            }

            protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args) {
                if (!_isDrag) {
                    drag.Clear();
                    _isDrag = true;
                    foreach (var o in DragAndDrop.objectReferences) {
                        IInfo item;
                        if (ObjectToItem(o, out item) && item != null)
                            drag.Add(item);
                    }
                }

                DragAndDropVisualMode visualMode = DragAndDropVisualMode.None;

                if (args.performDrop || drag.Count == 0)
                    _isDrag = false;

                if (drag.Count == 0)
                    return DragAndDropVisualMode.Rejected;

                if (args.parentItem == null)
                    args.parentItem = root.item;

                IInfo parent = root.item.id == args.parentItem.id ? root : root.Find(args.parentItem.id);

                if (parent != null && !drag.All(x => CanBeChild(parent, x)))
                    return DragAndDropVisualMode.Rejected;

                switch (args.dragAndDropPosition) {
                    case DragAndDropPosition.UponItem:
                        if (parent.isItemKind || drag.Contains(x => x.isFolderKind && x.asFolderKind.IsParentOf(parent.asFolderKind)))
                            return DragAndDropVisualMode.Rejected;

                        if (args.performDrop) {
                            if (parent != null && parent.isFolderKind && drag.All(x => CanBeChild(parent, x))) {
                                int index = parent.asFolderKind.items.Count;
                                foreach (IInfo d in drag)
                                    PutInFolder(d, parent.asFolderKind, index++);

                                UpdatePath(drag);
                                BakeCollections();
                                onChanged();
                                Reload();
                            }
                        }
                        return DragAndDropVisualMode.Move;
                    case DragAndDropPosition.OutsideItems:
                    case DragAndDropPosition.BetweenItems:
                        if (parent.isFolderKind) {
                            if (args.performDrop) {
                                if (parent != null && parent.isFolderKind && drag.All(x => CanBeChild(parent, x))) {
                                    int index = 0;
                                    foreach (IInfo d in drag)
                                        PutInFolder(d, parent.asFolderKind, args.insertAtIndex + (index++));
                                    UpdatePath(drag);
                                    BakeCollections();
                                    onChanged();
                                    Reload();
                                }
                            }
                        }
                        return DragAndDropVisualMode.Move;
                }
                return visualMode;
            }

            protected bool PutInFolder(IInfo item, FolderInfo folder) {
                if (item != folder) {
                    if (item.parent != null)
                        item.parent.items.Remove(item);
                    item.parent = folder;
                    folder.items.Add(item);
                    return true;
                }
                return false;
            }
            protected bool PutInFolder(IInfo item, FolderInfo folder, int index) {
                if (item != folder) {
                    if (item.parent == folder && folder.items.IndexOf(item) < index)
                        index--;

                    if (item.parent != null)
                        item.parent.items.Remove(item);
                    item.parent = folder;

                    folder.items.Insert(Mathf.Clamp(index, 0, folder.items.Count), item);
                    return true;
                }

                return false;
            }

            public FolderInfo FindFolder(string path) {
                if (string.IsNullOrEmpty(path))
                    return null;

                FolderInfo result = root;
                foreach (string folder in path.Split('/')) {
                    if (string.IsNullOrEmpty(folder))
                        return null;
                    result = (FolderInfo) result.items.Find(x => x is FolderInfo && (x as FolderInfo).item.displayName == folder);
                    if (result == null)
                        return null;
                }

                return result;
            }

            void BakeCollections() {
                itemCollection.Clear();
                folderCollection.Clear();
                foreach (IInfo i in root.GetAllChild()) {
                    if (i.isItemKind) itemCollection.Add((i.asItemKind).content);
                    else folderCollection.Add((i.asFolderKind).content);
                }
            }

            protected void UpdatePath(List<IInfo> items) {
                List<IInfo> toUpdatePath = new List<IInfo>();
                foreach (IInfo item in items) {
                    toUpdatePath.Add(item);
                    if (item.isFolderKind) toUpdatePath.AddRange(item.asFolderKind.GetAllChild());
                }
                toUpdatePath.Distinct();
                toUpdatePath.Where(x => x.parent != null).ForEach(x => SetPath(x, x.parent.fullPath));
            }

            bool isContextOnItem = false;
            protected override void ContextClicked() {
                if (isContextOnItem) {
                    isContextOnItem = false;
                    return;
                }

                GenericMenu menu = new GenericMenu();

                ContextMenu(menu, new List<IInfo>());
                menu.ShowAsContext();
            }

            protected override void ContextClickedItem(int id) {
                isContextOnItem = true;

                GenericMenu menu = new GenericMenu();
                List<IInfo> selection = GetSelection().Select(x => info.Get(x)).Where(x => x != null).ToList();
                ContextMenu(menu, selection);

                menu.ShowAsContext();
            }

            public abstract void ContextMenu(GenericMenu menu, List<IInfo> selected);

            public class ItemInfo : IInfo {
                public I content;

                public ItemInfo(int depth, FolderInfo parent = null) {
                    item = new TreeViewItem(0, depth, "Item");
                    this.parent = parent;
                }
            }

            public class FolderInfo : IInfo {
                public F content;

                public FolderInfo(int depth, FolderInfo parent = null) {
                    item = new TreeViewItem(0, depth, "Folder");
                    this.parent = parent;
                }

                public List<IInfo> items = new List<IInfo>();

                public List<IInfo> GetAllChild() {
                    List<IInfo> result = new List<IInfo>();
                    foreach (IInfo item in items) {
                        result.Add(item);
                        if (item is FolderInfo)
                            result.AddRange((item as FolderInfo).GetAllChild());
                    }
                    return result;
                }

                public IInfo Find(int id) {
                    foreach (IInfo item in items) {
                        if (item.item.id == id)
                            return item;
                        if (item is FolderInfo) {
                            IInfo i = (item as FolderInfo).Find(id);
                            if (i != null)
                                return i;
                        }
                    }
                    return null;
                }

                public bool IsChildOf(FolderInfo folder) {
                    FolderInfo current = parent;
                    while (current != null) {
                        if (current == folder)
                            return true;
                        current = current.parent;
                    }
                    return false;
                }

                public bool IsParentOf(FolderInfo folder) {
                    return folder.IsChildOf(this);
                }
            }

            public abstract class IInfo {
                public TreeViewItem item;
                public FolderInfo parent;

                public string path {
                    get {
                        return parent != null ? parent.fullPath : "";
                    }
                }
                public string fullPath {
                    get {
                        if (parent == null) return "";
                        string path = this.path;
                        if (path.Length > 0)
                            return path + '/' + name;
                        return name;
                    }
                }
                public string name {
                    get {
                        return item.displayName;
                    }
                }

                public int index {
                    get {
                        if (parent != null)
                            return parent.items.IndexOf(this);
                        return -1;
                    }
                }

                public bool isItemKind {
                    get {
                        return this is ItemInfo;
                    }
                }
                public bool isFolderKind {
                    get {
                        return this is FolderInfo;
                    }
                }

                public ItemInfo asItemKind {
                    get {
                        return this as ItemInfo;
                    }
                }
                public FolderInfo asFolderKind {
                    get {
                        return this as FolderInfo;
                    }
                }

                public override string ToString() {
                    return (isItemKind ? "I:" : "F:") + fullPath;
                }
            }
        }

        public abstract class HierarchyList<I> : HierarchyList<I, TreeFolder> {
            static Texture2D folderIcon;

            public HierarchyList(List<I> collection, List<TreeFolder> folders, TreeViewState state) : base(collection, folders, state) {}

            public override TreeFolder CreateFolder() {
                return new TreeFolder();
            }

            public override void DrawFolder(Rect rect, FolderInfo info) {
                if (folderIcon == null)
                    folderIcon = EditorIcons.GetIcon("Folder");
                Rect _rect = new Rect(rect.x, rect.y, 16, rect.height);
                GUI.DrawTexture(_rect, folderIcon);
                _rect = new Rect(rect.x + 16, rect.y, rect.width - 16, rect.height);
                GUI.Label(_rect, info.item.displayName);
            }

            public override string GetPath(TreeFolder element) {
                return element.path;
            }

            public override int GetUniqueID(TreeFolder element) {
                return element.GetHashCode();
            }

            public override void SetPath(TreeFolder element, string path) {
                element.path = path;
            }

            public override string GetName(TreeFolder element) {
                return element.name;
            }

            public override void SetName(TreeFolder element, string name) {
                element.name = name;
            }
        }

        public abstract class NonHierarchyList<I> : HierarchyList<I, TreeFolder> {
            static Texture2D folderIcon;

            public NonHierarchyList(List<I> collection, TreeViewState state, string name = null) : base(collection, null, state, name) {}

            public override void ContextMenu(GenericMenu menu, List<IInfo> selected) {
                selected = selected.Where(x => x.isItemKind).ToList();
                if (selected.Count == 0) 
                    menu.AddItem(new GUIContent("New Item"), false, () => AddNewItem(headFolder, null));
                else
                    menu.AddItem(new GUIContent("Remove"), false, () => Remove(selected.ToArray()));
            }

            public FolderInfo rootFolder {
                get {
                    return AddFolder("");
                }
            }
            public FolderInfo headFolder {
                get {
                    return string.IsNullOrEmpty(listName) ? rootFolder : AddFolder("root");
                }
            }

            public override TreeFolder CreateFolder() {
                return new TreeFolder();
            }

            public override void DrawFolder(Rect rect, FolderInfo info) {
                if (folderIcon == null)
                    folderIcon = EditorIcons.GetIcon("Folder");
                Rect _rect = new Rect(rect.x, rect.y, 16, rect.height);
                GUI.DrawTexture(_rect, folderIcon);
                _rect = new Rect(rect.x + 16, rect.y, rect.width - 16, rect.height);
                GUI.Label(_rect, listName);
            }

            protected override void RowGUI(RowGUIArgs args) {
                if (string.IsNullOrEmpty(listName)) {
                    IInfo i = info.Get(args.item.id);
                    if (i == null) return;
                    if (i.isItemKind) DrawItem(args.rowRect, i.asItemKind);
                } else
                    base.RowGUI(args);
            }

            public override string GetPath(TreeFolder element) {
                return "";
            }

            public override string GetPath(I element) {
                return string.IsNullOrEmpty(listName) ? "" : "root";
            }

            public override string GetName(TreeFolder element) {
                return string.IsNullOrEmpty(listName) ? "" : "root";
            }

            public override string GetName(I element) {
                return element.GetHashCode().ToString();
            }

            public override int GetUniqueID(TreeFolder element) {
                return -2;
            }

            public override void SetPath(TreeFolder element, string path) {
                return;
            }

            public override void SetPath(I element, string path) {
                return;
            }
        }

        public class SearchPanel {
            static GUIStyle _searchStyle, _searchXStyle, _keyItemStyle;
            static GUIStyle searchStyle {
                get {
                    if (_searchStyle == null)
                        _searchStyle = GUI.skin.FindStyle("ToolbarSeachTextField");
                    return _searchStyle;
                }
            }
            static GUIStyle searchXStyle {
                get {
                    if (_searchXStyle == null)
                        _searchXStyle = GUI.skin.FindStyle("ToolbarSeachCancelButton");
                    return _searchXStyle;
                }
            }
            public static GUIStyle keyItemStyle {
                get {
                    if (_keyItemStyle == null) {
                        _keyItemStyle = new GUIStyle(EditorStyles.label);
                        _keyItemStyle.richText = true;
                    }
                    return _keyItemStyle;
                }
            }

            string search;
            public string value {
                get {
                    return search;
                }
                set {
                    search = value;
                }
            }

            public SearchPanel(string search) {
                this.search = search;
            }

            public void OnGUI(Action<string> onChanged, params GUILayoutOption[] options) {
                using (new GUIHelper.Change(() => onChanged(search))) {
                    search = EditorGUILayout.TextField(search, searchStyle, options);
                    if (GUILayout.Button("", searchXStyle)) {
                        search = "";
                        EditorGUI.FocusTextInControl("");
                    }
                }
            }

            public static string Format(string text, string search) {
                int index = text.ToLower().IndexOf(search.ToLower());
                if (index < 0) return text;
                string result = text.Substring(0, index);
                result += string.Format(Styles.highlightStrongBlue, text.Substring(index, search.Length));
                result += text.Substring(index + search.Length);
                return result;
            }
        } 
    }

    public static class EditorIcons
    {
        static Dictionary<string, Texture2D> icons = new Dictionary<string, Texture2D>();

        public static Texture2D GetIcon(string name)
        {
            if (!icons.ContainsKey(name))
                icons.Add(name, null);
            if (icons[name] == null)
                icons[name] = FindIcon(name);
            if (icons[name] == null)
                icons[name] = FindIcon2(name);
            return icons[name];
        }

        static Texture2D FindIcon(string name)
        {
            return EditorGUIUtility.Load(string.Format("Icons/{0}.png", name)) as Texture2D;
        }

        static Texture2D FindIcon2(string name)
        {
            return EditorGUIUtility.Load(string.Format("Icons2/{0}.png", name)) as Texture2D;
        }
    }
}