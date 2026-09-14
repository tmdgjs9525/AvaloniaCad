using Avalonia.Container.Util;
using EyeContainer.Core.Interfaces;
using EyeContainer.Core.Parameter;
using Avalonia.Controls;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Avalonia.Container.Dialog
{
    internal class DialogService : IDialogService, IDialogRegister
    {
        private readonly Dictionary<string, (Type View, Type ViewModel)> _viewViewModelDictionary = new();

        internal DialogService()
        {
            // 생성자 간소화 - 의존성은 DI 확장 메서드에서 처리
        }
        public void ShowDialog(string viewName, Parameters? parameters = null, Action<IDialogResult?>? callback = null, StartPosition? startPosition = null)
        {
            // TODO : 로깅
            IsViewNameValid(viewName);

            DialogBase dialogBase = new(startPosition);

            SetDialogViewAndViewModel(viewName, parameters, dialogBase, callback);

            var owner = WindowUtil.GetActiveWindow();

            if (owner is null)
            {
                throw new ArgumentNullException("Active Window is null. Dialog must have Owner Window");
            }

            dialogBase.ShowDialog(owner);
        }

        public Task<IDialogResult?> ShowDialogAsync(string viewName, Parameters? parameters = null, StartPosition? startPosition = null)
        {
            // 1. TaskCompletionSource 생성
            var tcs = new TaskCompletionSource<IDialogResult?>();

            // 2. 기존 ShowDialog 호출 (콜백을 연결)
            ShowDialog(viewName, parameters, (result) =>
            {
                // 3. 콜백이 오면 Task의 결과를 설정 (await가 끝남)
                tcs.SetResult(result);
            }, startPosition);

            // 4. Task 반환
            return tcs.Task;
        }
        
        public void Show(string viewName, Parameters? parameters = null, StartPosition? startPosition = null)
        {
            //TODO : 로깅
            IsViewNameValid(viewName);

            DialogBase dialogBase = new(startPosition);

            SetDialogViewAndViewModel(viewName, parameters, dialogBase);

            dialogBase.Show();
        }

        private void SetDialogViewAndViewModel(string viewName, Parameters? parameters, DialogBase dialogBase, Action<IDialogResult>? callback = null)
        {
            var vm = Ioc.Default.GetRequiredService(_viewViewModelDictionary[viewName].Item2) ?? throw new ArgumentNullException($"Can't find '{viewName}Model from Di Container");

            var control = (Ioc.Default.GetRequiredService(_viewViewModelDictionary[viewName].Item1) as UserControl) ?? throw new ArgumentNullException($"Can't find '{viewName}' from DI Container. The view must be a UserControl");

            //Di Container에서 찾은 타입 가져와서 넣어주기
            control.DataContext = vm;

            ContentControl contentControl = FindChild<ContentControl>(dialogBase, "dialogContent") ?? throw new ArgumentNullException("dialogContent를 찾을 수 없음");

            contentControl.Content = control;

            DialogResult dialogResult = new();


            if (vm is IDialogAware dialogAware)
            {
                dialogAware.OnDialogOpened(parameters ?? new Parameters());

                //한번 호출되면 해제하기 위해 할당 후 등록
                Action<IDialogResult> requestCloseHandler = null!;
                requestCloseHandler = (result) =>
                {
                    if (dialogAware.CanCloseDialog())
                    {
                        dialogResult = (DialogResult)result;
                        dialogAware.OnDialogClosed();
                        dialogBase.Close();
                        callback?.Invoke(result);
                        dialogAware.RequestClose -= requestCloseHandler;
                        contentControl.Content = null;
                    }
                };

                dialogAware.RequestClose += requestCloseHandler;
            }
        }

        public void AddTransientDialog<TView, TViewModel>() where TView : Control
                                                            where TViewModel : IViewModelBase, IDialogAware
        {
            _viewViewModelDictionary[typeof(TView).Name] = (typeof(TView), typeof(TViewModel));

        }

        public void AddSingletonDialog<TView, TViewModel>() where TView : Control
                                                            where TViewModel : IViewModelBase, IDialogAware
        {
            _viewViewModelDictionary[typeof(TView).Name] = (typeof(TView), typeof(TViewModel));

        }

        private void IsViewNameValid(string viewName)
        {
            if (_viewViewModelDictionary.ContainsKey(viewName) == false)
            {
                throw new ArgumentNullException($"Can't find '{viewName}' from _viewDictionary ");
            }
        }


        // 특정 이름의 자식 컨트롤을 찾는 제네릭 메서드
        private T? FindChild<T>(Control parent, string name) where T : Control
        {
            // 부모가 ContentControl이라면 Content를 가져옴
            if (parent is ContentControl contentControl)
            {
                // Avalonia의 Content는 object? 이므로 Control로 캐스팅 (WPF와 동일)
                var content = contentControl.Content as Control;
                if (content == null)
                {
                    // Dialog가 UserControl이 아닐 수도 있으니,
                    // Content가 Control이 아니면 검색을 중단하거나 예외를 발생시킵니다.
                    // (원본 코드의 throw 로직 유지)
                    throw new ArgumentException("Dialog의 Content는 Control(UserControl 등) 이여야 합니다.");
                }
                parent = content;
            }

            // 자식 컨트롤을 순차적으로 검색 (WPF: VisualTreeHelper -> Avalonia: GetVisualChildren)
            foreach (var visualChild in parent.GetVisualChildren())
            {
                // GetVisualChildren은 IVisual을 반환하므로 Control로 캐스팅
                if (visualChild is Control child)
                {
                    // 자식이 찾고자 하는 타입과 이름인지 확인
                    // (WPF: FrameworkElement.NameProperty -> Avalonia: child.Name)
                    if (child is T typedChild && child.Name == name)
                    {
                        return typedChild;
                    }

                    // 하위 요소가 또 다른 부모 컨트롤을 가지고 있을 수 있으므로 재귀적으로 검색
                    T? foundChild = FindChild<T>(child, name);
                    if (foundChild != null)
                    {
                        return foundChild;
                    }
                }
            }

            return null;
        }


    }
}