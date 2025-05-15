using System;
using System.Collections.Generic;
using FinalFactory.Logging;
using FinalFactory.Mathematics;
using FinalFactory.Mathematics.Units;
using FinalFactory.Observable;
using UnityEngine;
using UnityEngine.UIElements;

namespace FinalFactory.UIElements
{
    public class FinalProgressbar : ComplexElement
    {
        public const string ClassName = "progress-bar";
        public const string ClassNameEmpty = ClassName + "--empty";
        public const string ClassNameFull = ClassName + "--full";
        public const string ClassNameContainer = ClassName + "__container";
        public const string ClassNameTitle = ClassName + "__title";
        public const string ClassNameTitleContainer = ClassName + "__title-container";
        public const string ClassNameInfill = ClassName + "__infill";
        public const string ClassNameInfillMain = ClassName + "__infill--main";
        public const string ClassNameInfillOpposite = ClassName + "__infill--opposite";
        public const string ClassNameBackground = ClassName + "__background";
        public const string ClassNameMarker = ClassName + "__marker";

        protected const int LayoutUpdateCodePartial = 1;

        private static readonly Log Log = LogManager.GetLogger(typeof(FinalProgressbar));

        private readonly VisualElement _bar;
        private readonly VisualElement _infillMain;
        private readonly VisualElement _infillOpposite;
        private readonly Label _title;
        private readonly List<(float, VisualElement)> _markers = new();

        public readonly IProperty<string> TitleProperty;
        public readonly IProperty<ProgressbarFillDirection> FillDirectionProperty;
        public readonly IProperty<ProgressbarFillType> FillTypeProperty;
        public readonly IProperty<RangeF> RangeProperty;
        public readonly IProperty<float> OppositeValueProperty;
        public readonly IProperty<float> MinDisplayPixelProperty;

        public FinalProgressbar()
        {
            PerformDetachedLayoutUpdates = false;

            AddToClassList(ClassName);
            Resources.Load<VisualTreeAsset>("FinalFactory/UXML/FinalProgressbar").CloneTree(this);
            styleSheets.Add(Resources.Load<StyleSheet>("FinalFactory/USS/FinalProgressbar"));
            
            _bar = this.Q("bar");
            _infillMain = this.Q("infill-main");
            _infillOpposite = this.Q("infill-opposite");
            _title = this.Q<Label>("title");

            TitleProperty = new B<string>(() => _title.text, s => _title.text = s);
            RangeProperty = new P<RangeF>(new RangeF(0f, 0f, 100f), (_,_)  => UpdateLayout(LayoutUpdateCodePartial));
            OppositeValueProperty = new P<float>(100f, (_,_)  => UpdateLayout(LayoutUpdateCodePartial));
            FillDirectionProperty = new P<ProgressbarFillDirection>(ProgressbarFillDirection.FromLeft, (_,_)  => PerformLayout());
            FillTypeProperty = new P<ProgressbarFillType>(ProgressbarFillType.Slide, (_,_)  => PerformLayout());
            MinDisplayPixelProperty = new P<float>(1f, (_,_)  => PerformLayout());
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        public float Value
        {
            get => RangeProperty.Value.Value;
            set => RangeProperty.Value = new RangeF(value, RangeProperty.Value.Min, RangeProperty.Value.Max);
        }
        
        public float LowValue
        {
            get => RangeProperty.Value.Min;
            set => RangeProperty.Value = new RangeF(RangeProperty.Value.Value, value, RangeProperty.Value.Max);
        }
        
        public float HighValue
        {
            get => RangeProperty.Value.Max;
            set => RangeProperty.Value = new RangeF(RangeProperty.Value.Value, RangeProperty.Value.Min, value);
        }
        
        public string Title
        {
            get => TitleProperty.Value;
            set => TitleProperty.Value = value;
        }
        
        public ProgressbarFillDirection FillDirection
        {
            get => FillDirectionProperty.Value;
            set => FillDirectionProperty.Value = value;
        }
        
        public ProgressbarFillType FillType
        {
            get => FillTypeProperty.Value;
            set => FillTypeProperty.Value = value;
        }
        
        public float OppositeValue
        {
            get => OppositeValueProperty.Value;
            set => OppositeValueProperty.Value = value;
        }
        
        public float MinDisplayPixel
        {
            get => MinDisplayPixelProperty.Value;
            set => MinDisplayPixelProperty.Value = value;
        }
        
        public int MarkerCount => _markers.Count;
        
        public int AddMarker(float value, string name = null)
        {
            var valuePercentage = value / RangeProperty.Value.Span * 100;
            var index = _markers.Count;
            
            var markerContainer = new VisualElement();
            markerContainer.name = "Marker--" + name;
            markerContainer.style.width = 0;
            markerContainer.style.height = Length.Percent(100);
            markerContainer.style.position = Position.Absolute;
            markerContainer.style.left = Length.Percent(valuePercentage);
            
            var marker = new VisualElement();
            marker.AddToClassList(ClassNameMarker);
            if (!string.IsNullOrWhiteSpace(name))
            {
                marker.AddToClassList($"{ClassNameMarker}--{name}");
            }
            
            markerContainer.Add(marker);
            
            _markers.Add((value, markerContainer));
            _bar.Add(markerContainer);
            return index;
        }
        
        public void AddOrUpdateMarker(float value, string name)
        {
            for (var i = 0; i < _markers.Count; i++)
            {
                if (_markers[i].Item2.name == "Marker--" + name)
                {
                    UpdateMarker(i, value);
                    return;
                }
            }
            AddMarker(value, name);
        }
        
        public bool ContainsMarker(string name)
        {
            for (var i = 0; i < _markers.Count; i++)
            {
                if (_markers[i].Item2.name == "Marker--" + name)
                {
                    return true;
                }
            }
            return false;
        }
        
        public void UpdateMarker(int index, float value)
        {
            if (index >= 0 && index < _markers.Count)
            {
                var valuePercentage = value / RangeProperty.Value.Span * 100;
                var (oldValue, marker) = _markers[index];
                marker.style.left = Length.Percent(valuePercentage);
                _markers[index] = (value, marker);
            }
        }
        
        public bool RemoveMarker(int index)
        {
            if (index >= 0 && index < _markers.Count)
            {
                _markers[index].Item2.RemoveFromHierarchy();
                _markers.RemoveAt(index);
                return true;
            }
            return false;
        }
        
        public bool RemoveMarker(string name)
        {
            for (var i = 0; i < _markers.Count; i++)
            {
                if (_markers[i].Item2.name == "Marker--" + name)
                {
                    _markers[i].Item2.RemoveFromHierarchy();
                    _markers.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
        
        public void ClearMarkers()
        {
            foreach (var (_, marker) in _markers)
            {
                marker.RemoveFromHierarchy();
            }
            _markers.Clear();
        }
        
        private void OnGeometryChanged(GeometryChangedEvent e) => UpdateLayout(LayoutUpdateCodePartial);

        protected override void OnUpdateLayout(int updateCode)
        {
            base.OnUpdateLayout(updateCode);

            var dir = FillDirectionProperty.Value;
            var type = FillTypeProperty.Value;

            var centerH = dir == ProgressbarFillDirection.CenterHorizontal;
            var centerV = dir == ProgressbarFillDirection.CenterVertical;
            var center = centerH || centerV;
            if (updateCode == LayoutUpdateCodeFull)
            {
                if (type == ProgressbarFillType.Slide &&
                    (centerH || centerV))
                {
                    FillTypeProperty.Value = ProgressbarFillType.Grow;
                    Log.Warn("Final Progressbar does currently not support fill type slide combined with fill direction center.");
                    return;
                }
            }

            var fillPercentage = RangeProperty.Value.Percentage;
            var oppositePercentage = new Percentage(((RangeProperty.Value.Max - OppositeValueProperty.Value) / (double)RangeProperty.Value.Span).Clamp01());

            EnableInClassList(ClassNameEmpty, fillPercentage.IsZero);
            EnableInClassList(ClassNameFull, fillPercentage.IsFull);
            
            var distancePx = 0f;
            var distancePxOpposite = 0f;
            var layout = _bar.layout;
            var minDisplaySizePx = MinDisplayPixelProperty.Value;

            if (dir == ProgressbarFillDirection.FromLeft ||
                dir == ProgressbarFillDirection.FromRight ||
                centerH)
            {
                var width = layout.width;
                if (!width.IsNaN())
                {
                    if (fillPercentage.IsZero)
                    {
                        distancePx = width;
                    }
                    else
                    {
                        distancePx = width - Mathf.Max(width * fillPercentage, minDisplaySizePx);
                    }

                    if (oppositePercentage.IsZero)
                    {
                        distancePxOpposite = width;
                    }
                    else
                    {
                        distancePxOpposite = width - Mathf.Max(width * oppositePercentage, minDisplaySizePx);
                    }
                }

                if (distancePx < 0.0)
                    distancePx = 0f;
                
                if (distancePxOpposite < 0.0)
                    distancePxOpposite = 0f;
                
                if (type == ProgressbarFillType.Slide)
                {
                    switch (dir)
                    {
                        case ProgressbarFillDirection.FromLeft:
                            _infillMain.style.right = distancePx;
                            _infillOpposite.style.left = distancePxOpposite;
                            break;
                        case ProgressbarFillDirection.FromRight:
                            _infillMain.style.left = distancePx;
                            _infillOpposite.style.right = distancePxOpposite;
                            break;
                        case ProgressbarFillDirection.CenterHorizontal:
                            var halfDist = distancePx / 2;
                            _infillMain.style.right = halfDist;
                            _infillMain.style.left = halfDist;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    switch (dir)
                    {
                        case ProgressbarFillDirection.FromLeft:
                            _infillMain.style.right = distancePx;
                            _infillOpposite.style.left = distancePxOpposite;
                            break;
                        case ProgressbarFillDirection.FromRight:
                            _infillMain.style.left = distancePx;
                            _infillOpposite.style.right = distancePxOpposite;
                            break;
                        case ProgressbarFillDirection.CenterHorizontal:
                            var halfDist = distancePx / 2;
                            _infillMain.style.right = halfDist;
                            _infillMain.style.left = halfDist;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                _infillMain.style.top = 0;
                _infillMain.style.bottom = 0;
                _infillOpposite.style.top = 0;
                _infillOpposite.style.bottom = 0;
            }
            else
            {
                var height = layout.height;
                if (!height.IsNaN())
                {
                    if (fillPercentage.IsZero)
                    {
                        distancePx = height;
                    }
                    else
                    {
                        distancePx = height - Mathf.Max(height * fillPercentage, minDisplaySizePx);
                    }
                    
                    if (oppositePercentage.IsZero)
                    {
                        distancePxOpposite = height;
                    }
                    else
                    {
                        distancePxOpposite = height - Mathf.Max(height * oppositePercentage, minDisplaySizePx);
                    }
                }

                if (distancePx < 0.0)
                    distancePx = 0f;

                if (distancePxOpposite < 0.0)
                    distancePxOpposite = 0f;
                
                if (type == ProgressbarFillType.Slide)
                {
                    switch (dir)
                    {
                        case ProgressbarFillDirection.FromTop:
                            _infillMain.style.bottom = distancePx;
                            _infillOpposite.style.top = distancePxOpposite;
                            break;
                        case ProgressbarFillDirection.FromBottom:
                            _infillMain.style.top = distancePx;
                            _infillOpposite.style.bottom = distancePxOpposite;
                            break;
                        case ProgressbarFillDirection.CenterVertical:
                            var halfDist = distancePx / 2;
                            _infillMain.style.bottom = halfDist;
                            _infillMain.style.top = halfDist;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    switch (dir)
                    {
                        case ProgressbarFillDirection.FromTop:
                            _infillMain.style.marginBottom = distancePx;
                            _infillOpposite.style.marginTop = distancePxOpposite;
                            break;
                        case ProgressbarFillDirection.FromBottom:
                            _infillMain.style.marginTop = distancePx;
                            _infillOpposite.style.marginBottom = distancePxOpposite;
                            break;
                        case ProgressbarFillDirection.CenterVertical:
                            var halfDist = distancePx / 2;
                            _infillMain.style.marginBottom = halfDist;
                            _infillMain.style.marginTop = halfDist;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                _infillMain.style.left = 0;
                _infillMain.style.right = 0;
                _infillOpposite.style.left = 0;
                _infillOpposite.style.right = 0;
            }
            
            foreach (var (value, marker) in _markers)
            {
                var valuePercentage = value / RangeProperty.Value.Span * 100;
                marker.style.width = Length.Percent(valuePercentage);
            }
        }

        
        public new class UxmlFactory : UxmlFactory<FinalProgressbar, UxmlTraits>
        {
        }

        public new class UxmlTraits : ComplexElement.UxmlTraits
        {
            private readonly UxmlFloatAttributeDescription _oppositeValue = new()
            {
                name = "opposite-value",
                defaultValue = 100f
            };
            
            private readonly UxmlFloatAttributeDescription _lowValue = new()
            {
                name = "low-value",
                defaultValue = 0.0f
            };

            private readonly UxmlFloatAttributeDescription _value = new()
            {
                name = "value",
                defaultValue = 0.0f
            };

            private readonly UxmlFloatAttributeDescription _highValue = new()
            {
                name = "high-value",
                defaultValue = 100f
            };

            private readonly UxmlStringAttributeDescription _title = new()
            {
                name = "title"
            };

            private readonly UxmlEnumAttributeDescription<ProgressbarFillDirection> _fillDirection = new()
            {
                name = "fill-direction"
            };

            private readonly UxmlEnumAttributeDescription<ProgressbarFillType> _fillType = new()
            {
                name = "fill-type"
            };
            private readonly UxmlFloatAttributeDescription _minDisplayPixel = new()
            {
                name = "min-display-pixel"
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var e = (FinalProgressbar)ve;
                e.SuspendLayout();
                e.RangeProperty.Value = new RangeF(_value.GetValueFromBag(bag, cc), _lowValue.GetValueFromBag(bag, cc), _highValue.GetValueFromBag(bag, cc));
                e.FillDirectionProperty.Value = _fillDirection.GetValueFromBag(bag, cc);
                e.FillTypeProperty.Value = _fillType.GetValueFromBag(bag, cc);
                e.MinDisplayPixelProperty.Value = _minDisplayPixel.GetValueFromBag(bag, cc);
                e.OppositeValueProperty.Value = _oppositeValue.GetValueFromBag(bag, cc);
                var valueFromBag = _title.GetValueFromBag(bag, cc);
                if (!string.IsNullOrEmpty(valueFromBag))
                {
                    e.TitleProperty.Value = valueFromBag;
                }

                e.ResumeLayout();
            }
        }
    }
}