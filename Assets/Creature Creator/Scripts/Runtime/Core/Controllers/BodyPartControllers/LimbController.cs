﻿// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class LimbController : BodyPartController
    {
        #region Fields
        [Header("Limb")]
        [SerializeField] private GameObject movePrefab;
        [SerializeField] private Transform[] bones;
        [SerializeField] private Transform extremity;
        [SerializeField] private Vector3 rotationalOffset = new Vector3(90,0,0);
        private SkinnedMeshRenderer skinnedMeshRenderer;
        private MeshCollider meshCollider;
        #endregion

        #region Properties
        public LimbController FlippedLimb { get { return Flipped as LimbController; } }

        public Transform[] Bones { get { return bones; } }
        public Transform Extremity { get { return extremity; } }
        
        const float scrollWeigth = 5f;
        const float minSize = 0.1f;
        const float maxSize = 10f;
        #endregion

        #region Methods
        protected override void Awake()
        {
            base.Awake();

            skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            meshCollider = GetComponentInChildren<MeshCollider>();
        }
        protected override void Start()
        {
            base.Start();

            hover.OnEnter.AddListener(delegate
            {
                if (!Input.GetMouseButton(0))
                {
                    SetToolsVisibility(true);
                }
            });
            hover.OnExit.AddListener(delegate
            {
                if (!Input.GetMouseButton(0))
                {
                    SetToolsVisibility(false);
                }
            });

            Drag.OnRelease.AddListener(delegate
            {
                UpdateMeshCollider();
                FlippedLimb.UpdateMeshCollider();

                if (!hover.IsOver)
                {
                    SetToolsVisibility(false);
                }
            });
            Drag.OnDrag.AddListener(delegate
            {
                FlippedLimb.bones[bones.Length - 1].position = new Vector3(-bones[bones.Length - 1].position.x, bones[bones.Length - 1].position.y, bones[bones.Length - 1].position.z);
            });

            scroll.OnScrollUp.RemoveAllListeners();
            scroll.OnScrollDown.RemoveAllListeners();

            for (int i = 2; i < bones.Length; i++)
            {
                int index = i;

                Transform bone = bones[index];
                Transform flippedBone = FlippedLimb.bones[index];

                #region Interact
                Hover boneHover = bone.GetComponentInChildren<Hover>();
                boneHover.OnEnter.AddListener(delegate
                {
                    if (!Input.GetMouseButton(0))
                    {
                        CreatureCreator.Instance.CameraOrbit.Freeze();
                        SetToolsVisibility(true);
                    }
                });
                boneHover.OnExit.AddListener(delegate
                {
                    if (!Input.GetMouseButton(0))
                    {
                        CreatureCreator.Instance.CameraOrbit.Unfreeze();
                        SetToolsVisibility(false);
                    }
                });

                Drag boneDrag = bone.GetComponentInChildren<Drag>();
                boneDrag.OnPress.AddListener(delegate
                {
                    CreatureCreator.Instance.CameraOrbit.Freeze();

                    SetToolsVisibility(true);
                });
                boneDrag.OnDrag.AddListener(delegate
                {
                    flippedBone.position = new Vector3(-bone.position.x, bone.position.y, bone.position.z);
                });
                boneDrag.OnRelease.AddListener(delegate
                {
                    CreatureCreator.Instance.CameraOrbit.Unfreeze();

                    if (!boneHover.IsOver && !hover.IsOver)
                    {
                        SetToolsVisibility(false);
                    }

                    UpdateMeshCollider();
                    FlippedLimb.UpdateMeshCollider();
                });

                Scroll boneScroll = bone.GetComponentInChildren<Scroll>();
                if (boneScroll)
                {
                    boneScroll.OnScrollUp.AddListener(delegate
                    {
                        bone.localScale += Vector3.one * Time.deltaTime * scrollWeigth;
                        bone.localScale = bone.localScale.Clamp(minSize, maxSize);
                        flippedBone.localScale = bone.localScale;
                    });
                    boneScroll.OnScrollDown.AddListener(delegate
                    {
                        bone.localScale -= Vector3.one * Time.deltaTime * scrollWeigth;
                        bone.localScale = bone.localScale.Clamp(minSize, maxSize);
                        flippedBone.localScale = bone.localScale;
                    });
                }

                #endregion
            }

            UpdateMeshCollider();
            SetToolsVisibility(false);
        }
        protected virtual void LateUpdate()
        {
            for (int j = 1; j < bones.Length; j++)
            {
                bones[j - 1].rotation = Quaternion.LookRotation(bones[j].position - bones[j - 1].position, Vector3.right) * Quaternion.Euler(rotationalOffset);

                //if (bones[j].transform.position.y <= 0) // Keeps all bones above ground.
                //{
                //    bones[j].transform.position = new Vector3(bones[j].transform.position.x, 0, bones[j].transform.position.z);
                //}
            }
            bones[bones.Length - 1].rotation = bones[bones.Length - 2].rotation;
        }

        private void SetToolsVisibility(bool visible)
        {
            for (int i = 1; i < bones.Length; i++)
            {
                bones[i].GetComponentInChildren<MeshRenderer>().enabled = visible;
            }
        }
        public void UpdateMeshCollider()
        {
            Mesh skinnedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(skinnedMesh);
            meshCollider.sharedMesh = skinnedMesh;
        }
        #endregion
    }
}