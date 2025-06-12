using System;
using System.Collections;
using System.Collections.Generic;
using RKStudios._UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Example {
    public class Demo : MonoBehaviour {
        [SerializeField] private UIDocument uiDoc;

        private int _activeIndex = 0;

        private VisualElement _rootEl;
        private VisualElement _press;
        private VisualElement _wrapperEl;
        private List<VisualElement> _slides;
        private VisualElement _previousButton;
        private VisualElement _nextButton;
        private VisualElement _githubButton;

        private void OnEnable() {
            // Get references
            _rootEl = uiDoc.rootVisualElement;
            _press = _rootEl.Q("press");
            _wrapperEl = _rootEl.Q("wrapper");
            
            // Hide press play
            _press.AddToClassList("press--hidden");
            
            // Build the view
            VisualElement view = _UI.Build<VisualElement>(@"
                <ui:VisualElement class=""wrapper__inner"">
                    <ui:VisualElement class=""shape shape--x""/>
                    <ui:VisualElement class=""shape shape--dots""/>
                    <ui:VisualElement class=""shape shape--wedge2""/>
                    <ui:VisualElement class=""shape shape--curly""/>
                    <ui:VisualElement class=""shape shape--circle""/>
                    <ui:VisualElement class=""shape shape--oval""/>
                    <ui:VisualElement class=""shape shape--oval2""/>
                    <ui:VisualElement class=""shape shape--rainbow""/>
                    <ui:VisualElement class=""shape shape--star""/>
                    <ui:VisualElement class=""shape shape--wavey""/>
                    <ui:VisualElement class=""shape shape--wedge""/>
                    <ui:VisualElement class=""shape shape--wedge3""/>
                    <ui:VisualElement class=""shape shape--wedge4""/>

                    <ui:VisualElement class=""built"">
                        <ui:Label class=""built__text heading-font"" text=""THIS VIEW BUILT AT RUNTIME WITH _UI"" />
                    </ui:VisualElement>

                    <ui:VisualElement class=""slide title-slide slide--active"">
                        <ui:Label class=""slide__title heading-font"" text=""_UI"" />
                        <ui:Label class=""slide__description"" text=""Simplify the creation of dynamic UI elements and the application of dynamic styles."" />
                    </ui:VisualElement>

                    <ui:VisualElement class=""slide elements-slide"">
                        <ui:VisualElement class=""slide__content"">
                            <ui:Label class=""slide__title heading-font"" text=""Elements"" />
                            <ui:Label class=""slide__description font-regular"" text=""Every element and attribute supported!"" />

                            <ui:VisualElement class=""code-block"">
                                <ui:Label class=""code-block__line"" text=""string title = &quot;Element Creation&quot;;"" />
                                <ui:Label class=""code-block__line"" text=""VisualElement slide = _UI.Build&lt;VisualElement&gt;(@&quot;"" />
                                <ui:Label class=""code-block__line"" text=""    &lt;ui:VisualElement class=&quot;slide&quot;&gt;"" />
                                <ui:Label class=""code-block__line"" text=""        &lt;ui:Label class=&quot;slide__title&quot; text=&quot;{title}&quot; /&gt;"" />
                                <ui:Label class=""code-block__line"" text=""        &lt;ui:Label class=&quot;slide__desc&quot; text=&quot;Wow, That was easy!&quot; /&gt;"" />
                                <ui:Label class=""code-block__line"" text=""    &lt;/ui:VisualElement&gt;"" />
                                <ui:Label class=""code-block__line"" text=""&quot;);"" />
                            </ui:VisualElement>
                        </ui:VisualElement>
                    </ui:VisualElement>

                    <ui:VisualElement class=""slide styles-slide"">
                        <ui:VisualElement class=""slide__content"">
                            <ui:Label class=""slide__title heading-font"" text=""Styles"" />
                            <ui:Label class=""slide__description"" text=""Every style property supported!"" />

                            <ui:VisualElement class=""code-blocks"">
                                <ui:VisualElement class=""code-block"">
                                    <ui:Label text=""_UI.Style(el, @&quot;"" />
                                    <ui:Label text=""    width: 200px;"" />
                                    <ui:Label text=""    height: 200px;"" />
                                    <ui:Label text=""    font-size: 20px;"" />
                                    <ui:Label text=""&quot;);"" />
                                </ui:VisualElement>

                                <ui:VisualElement class=""code-block code-block--middle"">
                                    <ui:Label text=""_UI.Style("" />
                                    <ui:Label text=""    el,"" />
                                    <ui:Label text=""    StyleProperty.Color,"" />
                                    <ui:Label text=""    &quot;blue&quot;"" />
                                    <ui:Label text=""&quot;);&quot;"" />
                                </ui:VisualElement>

                                <ui:VisualElement class=""code-block"">
                                    <ui:Label text=""_UI.Style("" />
                                    <ui:Label text=""    el,"" />
                                    <ui:Label text=""    &quot;border-color&quot;,"" />
                                    <ui:Label text=""    &quot;red&quot;"" />
                                    <ui:Label text=""&quot;);&quot;"" />
                                </ui:VisualElement>
                            </ui:VisualElement>
                        </ui:VisualElement>
                    </ui:VisualElement>

                    <ui:VisualElement class=""slide opensource-slide"">
                        <ui:Label class=""slide__title heading-font"" text=""Contributions Welcome"" />
                        <ui:Label class=""slide__description"" text=""_UI is fully open source library and contributions are always welcome!"" />
                        <ui:VisualElement class=""button"" name=""github"">
                            <ui:Label text=""Visit our GitHub"" />
                        </ui:VisualElement>
                    </ui:VisualElement>

                    <ui:VisualElement class=""slide-nav"">
                        <ui:VisualElement name=""previous"" class=""slide-nav__button slide-nav__button--previous""/>
                        <ui:VisualElement name=""next"" class=""slide-nav__button slide-nav__button--next""/>
                    </ui:VisualElement>
                </ui:VisualElement>
            ");

            // Add it to the document
            _wrapperEl.Add(view);
            
            // Get references to elements in the view
            _slides = _rootEl.Query(className: "slide").ToList();
            _previousButton = _rootEl.Q("previous");
            _nextButton = _rootEl.Q("next");
            _githubButton = _rootEl.Q("github");
            
            // Register listeners
            _previousButton.RegisterCallback<ClickEvent>(_ => StartCoroutine(ChangeSlide(-1)));
            _nextButton.RegisterCallback<ClickEvent>(_ => StartCoroutine(ChangeSlide(1)));
            _githubButton.RegisterCallback<ClickEvent>(_ => Application.OpenURL("https://github.com/robbyklein/_UI"));
                
            // Display the slides
            UpdateSlideClass();
            StartCoroutine(Enable());
        }

        private IEnumerator Enable() {
            yield return new WaitForSeconds(1f);
            _wrapperEl.AddToClassList("wrapper--active");
        }

        private IEnumerator ChangeSlide(int direction) {
            if (_slides.Count == 0) yield break;
            _slides[_activeIndex].RemoveFromClassList("slide--active");
            _activeIndex = (_activeIndex + direction + _slides.Count) % _slides.Count;
            yield return new WaitForSeconds(0.25f);
            _slides[_activeIndex].AddToClassList("slide--active");
        }

        private void UpdateSlideClass() {
            for (int i = 0; i < _slides.Count; i++) {
                if (i == _activeIndex) {
                    _slides[i].AddToClassList("slide--active");
                } else {
                    _slides[i].RemoveFromClassList("slide--active");
                }
            }
        }
    }
}
