using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.RoadNetwork.UIDocBind
{
    public class UIDocBindHelper
    {
        /// <summary>
        /// ユーザーデータをバインドするためのヘルパークラス
        /// 基本的にはこのクラスで要件は満たせるようにする
        /// </summary>
        public static class Helper
        {

            /// <summary>
            /// アクセッサーの取得補助を行う構造体
            /// ヒープ領域を使わないようにするために構造体
            /// 引数からテンプレート引数を省略出来る
            /// </summary>
            /// <typeparam name="TObject"></typeparam>
            public struct AccessorHelper<TObject>
            {
                public AccessorHelper(TObject _){}
                public IAccessor<TValue> GetOrCreate<TValue>(in TValue _, string propertyName)
                {
                    return GetOrCreateAccessor<TValue, TObject>(propertyName);
                }
                public IAccessor<TValue> GetOrCreate<TValue>(string propertyName)
                {
                    return GetOrCreateAccessor<TValue, TObject>(propertyName);
                }

            }

            /// <summary>
            /// アクセッサーの取得補助を行う関数
            /// 
            /// UIDocBind.Helper.A(userData).GetOrCreate(userData.Position, nameof(userData.Position))
            /// こういった記述が出来るようになる。引数から型情報を取得する
            /// </summary>
            /// <typeparam name="TObject"></typeparam>
            /// <param name="target"></param>
            /// <returns></returns>
            public static AccessorHelper<TObject> A<TObject>(TObject target)
            {
                return new AccessorHelper<TObject>(target);
            }

            /// <summary>
            /// プロパティへのアクセッサーを取得するor生成する
            /// </summary>
            /// <typeparam name="TValue"></typeparam>
            /// <typeparam name="TObject"></typeparam>
            /// <param name="propertyName"></param>
            /// <param name="target"></param>
            /// <returns></returns>
            private static IAccessor<TValue> GetOrCreateAccessor<TValue, TObject>(string propertyName)
            {
                return PropertyAccessorLib.GetOrCreate<TValue, TObject>(propertyName);
            }

            /// <summary>
            /// VisualElementにプロパティをバインドする
            /// </summary>
            /// <typeparam name="TValue"></typeparam>
            /// <param name="element"></param>
            /// <param name="accessor"></param>
            /// <param name="target"></param>
            public static void Bind<TValue>(BaseField<TValue> element, IAccessor<TValue> accessor, object target)
            {
                var data = new Data<TValue>(target, accessor);
                element.userData = data;
                element.RegisterCallback<ChangeEvent<TValue>>(ChangedCallBack);
                element.SetValueWithoutNotify(accessor.Get(target));
            }

            /// <summary>
            /// VisualElementにプロパティをバインドする
            /// アクセッサーの準備も一緒になった関数
            /// </summary>
            /// <typeparam name="TValue"></typeparam>
            /// <typeparam name="TObject"></typeparam>
            /// <param name="element"></param>
            /// <param name="propertyName"></param>
            /// <param name="target"></param>
            public static void Bind<TValue, TObject>(BaseField<TValue> element, string propertyName, TObject target)
            {
                var data = new Data<TValue>(target, PropertyAccessorLib.GetOrCreate<TValue, TObject>(propertyName));
                element.userData = data;
                element.RegisterCallback<ChangeEvent<TValue>>(ChangedCallBack);
            }

        }

        // プロパティ検索時のマスク
        private static readonly System.Reflection.BindingFlags bindMask = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty;

        /// <summary>
        /// プロパティに高速にアクセスするための機能を提供するインターフェース
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        public interface IAccessor<TValue>
        {
            TValue Get(object obj);
            void Set(object obj, in TValue value);
        }

        /// <summary>
        /// プロパティに高速にアクセスするための機能を提供するインターフェース
        /// アクセス対象のデータを持つ
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        public interface IAccessorWithData<TValue>
        {
            TValue Get();
            void Set(in TValue value);
        }

        /// <summary>
        /// 設定するユーザーデータ
        /// </summary>
        public class Data<TValue> : IAccessorWithData<TValue>
        {
            public Data(object element, IAccessor<TValue> accessor)
            {
                Element = element;
                Debug.Assert(element != null);

                Accessor = accessor;
                Debug.Assert(Accessor != null);

            }

            public readonly object Element;
            public readonly IAccessor<TValue> Accessor;

            public TValue Get()
            {
                return Accessor.Get(Element);
            }

            public void Set(in TValue value)
            {
                Accessor.Set(Element, value);
            }

        }

        /// <summary>
        /// プロパティに高速にアクセスするための機能を実装したクラス
        /// PropertyInfo.SetValue()/GetValue()だと遅いらしいので、これを使う
        /// クラスの各プロパティ分だけ用意する必要がある
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        public class PropertyAccessor<TValue, TObject> : IAccessor<TValue>
        {
            public PropertyAccessor(PropertyInfo propertyInfo)
            {
                getter = (Func<TObject, TValue>)Delegate.CreateDelegate(typeof(Func<TObject, TValue>), propertyInfo.GetGetMethod());
                setter = (Action<TObject, TValue>)Delegate.CreateDelegate(typeof(Action<TObject, TValue>), propertyInfo.GetSetMethod());
            }

            private readonly Func<TObject, TValue> getter;
            private readonly Action<TObject, TValue> setter;

            public TValue Get(object obj)
            {
                return getter((TObject)obj);
            }

            public void Set(object obj, in TValue value)
            {
                setter((TObject)obj, value);
            }
        }

        /// <summary>
        /// PropertyAccessorのインスタンスを管理するクラス
        /// 生成処理が重いので、生成済みのものを使いまわす
        /// </summary>
        public static class PropertyAccessorLib
        {
            /// <summary>
            /// PropertyAccessorのインスタンスを生成する
            /// ただし、生成済みのものがあればそれを返す
            /// </summary>
            /// <typeparam name="TValue"></typeparam>
            /// <typeparam name="TObject"></typeparam>
            /// <param name="propertyName"></param>
            /// <returns></returns>
            public static PropertyAccessor<TValue, TObject> GetOrCreate<TValue, TObject>(string propertyName)
            {
                var typeInfo = typeof(TObject);
                var targetProperty = typeInfo.GetProperty(propertyName, bindMask);
                Debug.Assert(targetProperty != null);

                string key = typeInfo.FullName + "." + propertyName;
                if (accessors.ContainsKey(key))
                {
                    return (PropertyAccessor<TValue, TObject>)accessors[key];
                }
                else
                {
                    var accessor = new PropertyAccessor<TValue, TObject>(targetProperty);
                    accessors[key] = accessor;
                    return accessor;
                }
            }

            private static Dictionary<string, object> accessors = new Dictionary<string, object>();
        }

        /// <summary>
        /// 値変更時のコールバック
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="e"></param>
        public static void ChangedCallBack<TValue>(ChangeEvent<TValue> e)
        {
            var data = (IAccessorWithData<TValue>)GetUserData(e);
            data.Set(e.newValue);
#if DEBUG
            //Debug.Log(data.Get());
#endif
        }

        // ユーザーデータを取得する
        public static object GetUserData<_T>(ChangeEvent<_T> e)
        {
            var visualElement = e.target as VisualElement;
            var userData = visualElement.userData;
            return userData;
        }

    }
}
