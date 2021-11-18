using System;
using System.Reflection;
using GameEditorWindow.Editor;
using Services.Runtime;
using UnityEditor;
using UnityEngine;

namespace Services.Editor
{
    public class ServicesEditorWindow : IGameEditorWindow
    {
        public GUIContent Icon { get; } = new GUIContent(EditorGUIUtility.IconContent("d_BlendTree Icon").image, "Service Manager");
        public int SortOrder { get; } = 0;

        private Vector2 _scrollPosSidebar;
        private Vector2 _scrollPosInspector;

        private FieldInfo[] _fields;
        private PropertyInfo[] _properties;
        
        private UnityEditor.Editor _activeServiceEditor;

        private IService _selectedService;
        
        public void OnFocused()
        {
            
        }

        public void OnFocusLost()
        {
            
        }

        public void ToolbarLeft()
        {
            
        }

        public void ToolbarRight()
        {
            if (_selectedService != null)
            {
                if (GUILayout.Button("Stop", EditorStyles.toolbarButton))
                {
                    _selectedService.Shutdown();
                    SelectService(null);
                    GUI.FocusControl(null);
                }
            }
        }

        public void MainContent()
        {
            EditorGUILayout.BeginHorizontal();
            {
                DrawSidebar();
                DrawInspector();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(250));
            {
                _scrollPosSidebar = EditorGUILayout.BeginScrollView(_scrollPosSidebar);
                {
                    foreach (var activeService in ServiceManager.EditorActiveServices)
                    {
                        GUI.color = _selectedService == activeService.Value ? Color.cyan : Color.white;
                        if (GUILayout.Button(activeService.Key.Name))
                        {
                            SelectService(activeService.Value);
                            GUI.FocusControl(null);
                        }
                        GUI.color = Color.white;
                    }
                }
                EditorGUILayout.EndScrollView();
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndVertical();
        }

        private void SelectService(IService service)
        {
            _selectedService = service;
            
            if (service == null)
            {
                _fields = null;
                _properties = null;
                return;
            }

            _fields = service.GetType().GetFields();
            _properties = service.GetType().GetProperties();
        }

        private void DrawInspector()
        {
            EditorGUILayout.BeginVertical("box");
            {
                _scrollPosInspector = EditorGUILayout.BeginScrollView(_scrollPosInspector);
                {
                    if (_selectedService != null)
                    {
                        DrawInspector(_selectedService);
                    }
                }
                EditorGUILayout.EndScrollView();
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawInspector(IService instance)
        {
            var type = instance.GetType();
            
            GUILayout.Label($"Type: {type}", EditorStyles.boldLabel);
            
            foreach (var fieldInfo in _fields)
            {
                if (fieldInfo.IsPrivate)
                {
                    EditorGUILayout.LabelField(fieldInfo.Name, fieldInfo.GetValue(instance).ToString());
                    continue;
                }
                
                fieldInfo.SetValue(instance, MakeFieldForType(fieldInfo.FieldType, fieldInfo.Name, fieldInfo.GetValue(instance)));
            }
            
            EditorGUILayout.Separator();

            foreach (var propertyInfo in _properties)
            {
                if (propertyInfo.SetMethod!= null && propertyInfo.SetMethod.IsPublic)
                {
                    propertyInfo.SetValue(instance, MakeFieldForType(propertyInfo.PropertyType, propertyInfo.Name, propertyInfo.GetValue(instance)));
                }
                else
                {
                    EditorGUILayout.LabelField(propertyInfo.Name, propertyInfo.GetValue(instance).ToString());
                }
            }
        }

        private static object MakeFieldForType(Type type, string label, object value)
        {
            T F<T>(Func<string, T, GUILayoutOption[], T> fn)
            {
                return fn(label, (T) value, null);
            }

            if (type == typeof(bool))
                return F<bool>(EditorGUILayout.Toggle);
            if (type == typeof(int))
                return F<int>(EditorGUILayout.IntField);
            if (type == typeof(long))
                return F<long>(EditorGUILayout.LongField);
            if (type == typeof(float))
                return F<float>(EditorGUILayout.FloatField);
            if (type == typeof(double))
                return F<double>(EditorGUILayout.DoubleField);
            if (type == typeof(string))
                return F<string>(EditorGUILayout.TextField);

            throw new ArgumentException(nameof(type));
        }
    }
}