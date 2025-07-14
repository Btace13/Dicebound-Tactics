using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

/* Author: Josh H.
 * Procedural UI Image
 * assetstore.joshh@gmail.com for feedback or questions
 */

namespace UnityEngine.UI.ProceduralImage
{

    [ExecuteInEditMode]
    [AddComponentMenu("UI/Procedural Image")]
    public class ProceduralImage : Image
    {
        [SerializeField] private float borderWidth;
        [SerializeField] private bool useBlur = false;
        [SerializeField, Range(0, 1)] private float blurStrength = 1f;
        private ProceduralImageModifier modifier;
        private static Material materialInstance;
        private static Material DefaultProceduralImageMaterial
        {
            get
            {
                if (materialInstance == null)
                {
                    materialInstance = new Material(Shader.Find("UI/Procedural UI Image"));
                }
                return materialInstance;
            }
            set
            {
                materialInstance = value;
            }
        }
        [SerializeField] private float falloffDistance = 1;

        public float BorderWidth
        {
            get
            {
                return borderWidth;
            }
            set
            {
                borderWidth = value;
                this.SetVerticesDirty();
            }
        }

        public float FalloffDistance
        {
            get
            {
                return falloffDistance;
            }
            set
            {
                falloffDistance = value;
                this.SetVerticesDirty();
            }
        }

        public bool UseBlur
        {
            get { return useBlur; }
            set { useBlur = value; SetMaterialProperties(); }
        }
        public float BlurStrength
        {
            get { return blurStrength; }
            set { blurStrength = Mathf.Clamp01(value); SetMaterialProperties(); }
        }

        protected ProceduralImageModifier Modifier
        {
            get
            {
                if (modifier == null)
                {
                    //try to get the modifier on the object.
                    modifier = this.GetComponent<ProceduralImageModifier>();
                    //if we did not find any modifier
                    if (modifier == null)
                    {
                        //Add free modifier
                        ModifierType = typeof(FreeModifier);
                    }
                }
                return modifier;
            }
            set
            {
                modifier = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the modifier. Adds a modifier of that type.
        /// </summary>
        /// <value>The type of the modifier.</value>
        public System.Type ModifierType
        {
            get
            {
                return Modifier.GetType();
            }
            set
            {
                if (modifier != null && modifier.GetType() != value)
                {
                    if (this.GetComponent<ProceduralImageModifier>() != null)
                    {
                        DestroyImmediate(this.GetComponent<ProceduralImageModifier>());
                    }
                    this.gameObject.AddComponent(value);
                    Modifier = this.GetComponent<ProceduralImageModifier>();
                    this.SetAllDirty();
                }
                else if (modifier == null)
                {
                    this.gameObject.AddComponent(value);
                    Modifier = this.GetComponent<ProceduralImageModifier>();
                    this.SetAllDirty();
                }
            }
        }

        override protected void OnEnable()
        {
            base.OnEnable();
            this.Init();
            SetMaterialProperties();
        }

        override protected void OnDisable()
        {
            base.OnDisable();
            this.m_OnDirtyVertsCallback -= OnVerticesDirty;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        void Init()
        {
            FixTexCoordsInCanvas();
            this.m_OnDirtyVertsCallback += OnVerticesDirty;
            this.preserveAspect = false;
            this.material = null;
            if (this.sprite == null)
            {
                this.sprite = EmptySprite.Get();
            }
        }

        protected void OnVerticesDirty()
        {
            if (this.sprite == null)
            {
                this.sprite = EmptySprite.Get();
            }
        }

        protected void FixTexCoordsInCanvas()
        {
            Canvas c = this.GetComponentInParent<Canvas>();
            if (c != null)
            {
                FixTexCoordsInCanvas(c);
            }
        }

        protected void FixTexCoordsInCanvas(Canvas c)
        {
            c.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2 | AdditionalCanvasShaderChannels.TexCoord3;
        }

#if UNITY_EDITOR
        public void Update()
        {
            if (!Application.isPlaying)
            {
                this.UpdateGeometry();
                SetMaterialProperties();
            }
        }
#endif

        /// <summary>
        /// Prevents radius to get bigger than rect size
        /// </summary>
        /// <returns>The fixed radius.</returns>
        /// <param name="vec">border-radius as Vector4 (starting upper-left, clockwise)</param>
        private Vector4 FixRadius(Vector4 vec)
        {
            Rect r = this.rectTransform.rect;
            vec = new Vector4(Mathf.Max(vec.x, 0), Mathf.Max(vec.y, 0), Mathf.Max(vec.z, 0), Mathf.Max(vec.w, 0));

            //Allocates mem
            //float scaleFactor = Mathf.Min(r.width / (vec.x + vec.y), r.width / (vec.z + vec.w), r.height / (vec.x + vec.w), r.height / (vec.z + vec.y), 1);
            //Allocation free:
            float scaleFactor = Mathf.Min(Mathf.Min(Mathf.Min(Mathf.Min(r.width / (vec.x + vec.y), r.width / (vec.z + vec.w)), r.height / (vec.x + vec.w)), r.height / (vec.z + vec.y)), 1f);
            return vec * scaleFactor;
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            base.OnPopulateMesh(toFill);
            EncodeAllInfoIntoVertices(toFill, CalculateInfo());
            SetMaterialProperties();
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            FixTexCoordsInCanvas();
        }

        ProceduralImageInfo CalculateInfo()
        {
            var r = GetPixelAdjustedRect();
            float pixelSize = 1f / Mathf.Max(0, falloffDistance);

            Vector4 radius = FixRadius(Modifier.CalculateRadius(r));

            float minside = Mathf.Min(r.width, r.height);

            ProceduralImageInfo info = new ProceduralImageInfo(r.width + falloffDistance, r.height + falloffDistance, falloffDistance, pixelSize, radius / minside, borderWidth / minside * 2, useBlur, blurStrength);

            return info;
        }

        void EncodeAllInfoIntoVertices(VertexHelper vh, ProceduralImageInfo info)
        {
            UIVertex vert = new UIVertex();

            Vector2 uv1 = new Vector2(info.width, info.height);
            Vector2 uv2 = new Vector2(EncodeFloats_0_1_16_16(info.radius.x, info.radius.y), EncodeFloats_0_1_16_16(info.radius.z, info.radius.w));
            Vector2 uv3 = new Vector2(info.borderWidth == 0 ? 1 : Mathf.Clamp01(info.borderWidth), info.pixelSize);

            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vert, i);

                vert.position += ((Vector3)vert.uv0 - new Vector3(0.5f, 0.5f)) * info.fallOffDistance;
                //vert.uv0 = vert.uv0;
                vert.uv1 = uv1;
                vert.uv2 = uv2;
                vert.uv3 = uv3;

                vh.SetUIVertex(vert, i);
            }
        }

        /// <summary>
        /// Encode two values between [0,1] into a single float. Each using 16 bits.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        float EncodeFloats_0_1_16_16(float a, float b)
        {
            Vector2 kDecodeDot = new Vector2(1.0f, 1f / 65535.0f);
            return Vector2.Dot(new Vector2(Mathf.Floor(a * 65534) / 65535f, Mathf.Floor(b * 65534) / 65535f), kDecodeDot);
        }

        private Material _instanceMaterial;
        public override Material material
        {
            get
            {
                if (base.m_Material != null)
                {
                    return base.material;
                }
                if (_instanceMaterial == null)
                {
                    _instanceMaterial = new Material(DefaultProceduralImageMaterial);
                    _instanceMaterial.name = "ProceduralImage_InstanceMaterial";
                }
                return _instanceMaterial;
            }
            set
            {
                base.material = value;
                _instanceMaterial = null;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_instanceMaterial != null)
            {
                if (Application.isPlaying)
                    Destroy(_instanceMaterial);
                else
                    DestroyImmediate(_instanceMaterial);
                _instanceMaterial = null;
            }
        }

        private void SetMaterialProperties()
        {
            var mat = material;
            if (mat != null)
            {
                if (useBlur)
                    mat.EnableKeyword("USE_BLUR");
                else
                    mat.DisableKeyword("USE_BLUR");
                mat.SetFloat("_UseBlur", useBlur ? 1f : 0f);
                mat.SetFloat("_BlurStrength", blurStrength);
            }
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            OnEnable();
        }

        /// <summary>
        /// Called when the script is loaded or a value is changed in the
        /// inspector (Called in the editor only).
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();

            //Don't allow negative numbers for fall off distance
            falloffDistance = Mathf.Max(0, falloffDistance);

            //Don't allow negative numbers for fall off distance
            borderWidth = Mathf.Max(0, borderWidth);
            blurStrength = Mathf.Clamp01(blurStrength);
            SetMaterialProperties();
        }
#endif
    }

    /// <summary>
    /// Contains all parameters of a proceduaral image
    /// </summary>
    public struct ProceduralImageInfo
    {
        public float width;
        public float height;
        public float fallOffDistance;
        public Vector4 radius;
        public float borderWidth;
        public float pixelSize;
        public bool useBlur;
        public float blurStrength;

        public ProceduralImageInfo(float width, float height, float fallOffDistance, float pixelSize, Vector4 radius, float borderWidth, bool useBlur, float blurStrength = 1f)
        {
            this.width = Mathf.Abs(width);
            this.height = Mathf.Abs(height);
            this.fallOffDistance = Mathf.Max(0, fallOffDistance);
            this.radius = radius;
            this.borderWidth = Mathf.Max(borderWidth, 0);
            this.pixelSize = Mathf.Max(0, pixelSize);
            this.useBlur = useBlur;
            this.blurStrength = Mathf.Clamp01(blurStrength);
        }
    }
}
