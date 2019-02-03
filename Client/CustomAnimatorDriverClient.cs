using System;
using UnityEngine;

public class CustomAnimatorDriverClient : MonoBehaviour
{
    // retrieved from Neuron API.
    public enum NeuronBones
    {
        Hips = 0,
        RightUpLeg = 1,
        RightLeg = 2,
        RightFoot = 3,
        LeftUpLeg = 4,
        LeftLeg = 5,
        LeftFoot = 6,
        Spine = 7,
        Spine1 = 8,
        Spine2 = 9,
        Spine3 = 10,
        Neck = 11,
        Head = 12,
        RightShoulder = 13,
        RightArm = 14,
        RightForeArm = 15,
        RightHand = 16,
        RightHandThumb1 = 17,
        RightHandThumb2 = 18,
        RightHandThumb3 = 19,
        RightInHandIndex = 20,
        RightHandIndex1 = 21,
        RightHandIndex2 = 22,
        RightHandIndex3 = 23,
        RightInHandMiddle = 24,
        RightHandMiddle1 = 25,
        RightHandMiddle2 = 26,
        RightHandMiddle3 = 27,
        RightInHandRing = 28,
        RightHandRing1 = 29,
        RightHandRing2 = 30,
        RightHandRing3 = 31,
        RightInHandPinky = 32,
        RightHandPinky1 = 33,
        RightHandPinky2 = 34,
        RightHandPinky3 = 35,
        LeftShoulder = 36,
        LeftArm = 37,
        LeftForeArm = 38,
        LeftHand = 39,
        LeftHandThumb1 = 40,
        LeftHandThumb2 = 41,
        LeftHandThumb3 = 42,
        LeftInHandIndex = 43,
        LeftHandIndex1 = 44,
        LeftHandIndex2 = 45,
        LeftHandIndex3 = 46,
        LeftInHandMiddle = 47,
        LeftHandMiddle1 = 48,
        LeftHandMiddle2 = 49,
        LeftHandMiddle3 = 50,
        LeftInHandRing = 51,
        LeftHandRing1 = 52,
        LeftHandRing2 = 53,
        LeftHandRing3 = 54,
        LeftInHandPinky = 55,
        LeftHandPinky1 = 56,
        LeftHandPinky2 = 57,
        LeftHandPinky3 = 58,

        NumOfBones
    }

    public Animator animator = null; // The animator component which receives the mocap data

    void Awake()
    {
        // If no animator was assigned, try to get one
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    public void ApplyMotion(byte[] data)
    {
        if (animator != null)
        {
            // apply motion to bones
            int i = 0;
            while (i < data.Length)
            {
                // bone id
                byte neuronBoneId = data[i++];

                byte rotationOrPosition = data[i++];
                // rotation
                if (rotationOrPosition == 0)
                {
                    byte[] rotationXBytes = new byte[4];
                    rotationXBytes[0] = data[i++];
                    rotationXBytes[1] = data[i++];
                    rotationXBytes[2] = data[i++];
                    rotationXBytes[3] = data[i++];
                    byte[] rotationYBytes = new byte[4];
                    rotationYBytes[0] = data[i++];
                    rotationYBytes[1] = data[i++];
                    rotationYBytes[2] = data[i++];
                    rotationYBytes[3] = data[i++];
                    byte[] rotationZBytes = new byte[4];
                    rotationZBytes[0] = data[i++];
                    rotationZBytes[1] = data[i++];
                    rotationZBytes[2] = data[i++];
                    rotationZBytes[3] = data[i++];

                    float rotationX = BitConverter.ToSingle(rotationXBytes, 0);
                    float rotationY = BitConverter.ToSingle(rotationYBytes, 0);
                    float rotationZ = BitConverter.ToSingle(rotationZBytes, 0);
                    
                    HumanBodyBones boneId = GetHumanBodyBoneFromNeuronBone((NeuronBones)neuronBoneId);

                    if (boneId != HumanBodyBones.Jaw)
                    {
                        SetRotation(animator, boneId, new Vector3(rotationX, rotationY, rotationZ));
                    }
                }
                // position
                else if (rotationOrPosition == 1)
                {
                    byte[] positionXBytes = new byte[4];
                    positionXBytes[0] = data[i++];
                    positionXBytes[1] = data[i++];
                    positionXBytes[2] = data[i++];
                    positionXBytes[3] = data[i++];
                    byte[] positionYBytes = new byte[4];
                    positionYBytes[0] = data[i++];
                    positionYBytes[1] = data[i++];
                    positionYBytes[2] = data[i++];
                    positionYBytes[3] = data[i++];
                    byte[] positionZBytes = new byte[4];
                    positionZBytes[0] = data[i++];
                    positionZBytes[1] = data[i++];
                    positionZBytes[2] = data[i++];
                    positionZBytes[3] = data[i++];

                    float positionX = BitConverter.ToSingle(positionXBytes, 0);
                    float positionY = BitConverter.ToSingle(positionYBytes, 0);
                    float positionZ = BitConverter.ToSingle(positionZBytes, 0);

                    HumanBodyBones boneId = GetHumanBodyBoneFromNeuronBone((NeuronBones)neuronBoneId);
                    if (boneId != HumanBodyBones.Jaw)
                    {
                        SetPosition(animator, boneId, new Vector3(positionX, positionY, positionZ));
                    }
                }
                // rotation and position
                else if (rotationOrPosition == 2)
                {
                    byte[] rotationXBytes = new byte[4];
                    rotationXBytes[0] = data[i++];
                    rotationXBytes[1] = data[i++];
                    rotationXBytes[2] = data[i++];
                    rotationXBytes[3] = data[i++];
                    byte[] rotationYBytes = new byte[4];
                    rotationYBytes[0] = data[i++];
                    rotationYBytes[1] = data[i++];
                    rotationYBytes[2] = data[i++];
                    rotationYBytes[3] = data[i++];
                    byte[] rotationZBytes = new byte[4];
                    rotationZBytes[0] = data[i++];
                    rotationZBytes[1] = data[i++];
                    rotationZBytes[2] = data[i++];
                    rotationZBytes[3] = data[i++];

                    float rotationX = BitConverter.ToSingle(rotationXBytes, 0);
                    float rotationY = BitConverter.ToSingle(rotationYBytes, 0);
                    float rotationZ = BitConverter.ToSingle(rotationZBytes, 0);

                    byte[] positionXBytes = new byte[4];
                    positionXBytes[0] = data[i++];
                    positionXBytes[1] = data[i++];
                    positionXBytes[2] = data[i++];
                    positionXBytes[3] = data[i++];
                    byte[] positionYBytes = new byte[4];
                    positionYBytes[0] = data[i++];
                    positionYBytes[1] = data[i++];
                    positionYBytes[2] = data[i++];
                    positionYBytes[3] = data[i++];
                    byte[] positionZBytes = new byte[4];
                    positionZBytes[0] = data[i++];
                    positionZBytes[1] = data[i++];
                    positionZBytes[2] = data[i++];
                    positionZBytes[3] = data[i++];

                    float positionX = BitConverter.ToSingle(positionXBytes, 0);
                    float positionY = BitConverter.ToSingle(positionYBytes, 0);
                    float positionZ = BitConverter.ToSingle(positionZBytes, 0);

                    HumanBodyBones boneId = GetHumanBodyBoneFromNeuronBone((NeuronBones)neuronBoneId);
                    if (boneId != HumanBodyBones.Jaw)
                    {
                        SetRotation(animator, boneId, new Vector3(rotationX, rotationY, rotationZ));
                        SetPosition(animator, boneId, new Vector3(positionX, positionY, positionZ));
                    }
                }
            }
        }
    }

    public static HumanBodyBones GetHumanBodyBoneFromNeuronBone(NeuronBones bone)
    {
        switch (bone)
        {
            case NeuronBones.Hips:
                return HumanBodyBones.Hips;
            case NeuronBones.RightUpLeg:
                return HumanBodyBones.RightUpperLeg;
            case NeuronBones.RightLeg:
                return HumanBodyBones.RightLowerLeg;
            case NeuronBones.RightFoot:
                return HumanBodyBones.RightFoot;
            case NeuronBones.LeftUpLeg:
                return HumanBodyBones.LeftUpperLeg;
            case NeuronBones.LeftLeg:
                return HumanBodyBones.LeftLowerLeg;
            case NeuronBones.LeftFoot:
                return HumanBodyBones.LeftFoot;
            case NeuronBones.Spine:
                return HumanBodyBones.Spine;
            case NeuronBones.Spine3:
                return HumanBodyBones.Chest;
            case NeuronBones.Neck:
                return HumanBodyBones.Neck;
            case NeuronBones.Head:
                return HumanBodyBones.Head;
            case NeuronBones.RightShoulder:
                return HumanBodyBones.RightShoulder;
            case NeuronBones.RightArm:
                return HumanBodyBones.RightUpperArm;
            case NeuronBones.RightForeArm:
                return HumanBodyBones.RightLowerArm;
            case NeuronBones.RightHand:
                return HumanBodyBones.RightHand;
            case NeuronBones.RightHandThumb1:
                return HumanBodyBones.RightThumbProximal;
            case NeuronBones.RightHandThumb2:
                return HumanBodyBones.RightThumbIntermediate;
            case NeuronBones.RightHandThumb3:
                return HumanBodyBones.RightThumbDistal;
            case NeuronBones.RightHandIndex1:
                return HumanBodyBones.RightIndexProximal;
            case NeuronBones.RightHandIndex2:
                return HumanBodyBones.RightIndexIntermediate;
            case NeuronBones.RightHandIndex3:
                return HumanBodyBones.RightIndexDistal;
            case NeuronBones.RightHandMiddle1:
                return HumanBodyBones.RightMiddleProximal;
            case NeuronBones.RightHandMiddle2:
                return HumanBodyBones.RightMiddleIntermediate;
            case NeuronBones.RightHandMiddle3:
                return HumanBodyBones.RightMiddleDistal;
            case NeuronBones.RightHandRing1:
                return HumanBodyBones.RightRingProximal;
            case NeuronBones.RightHandRing2:
                return HumanBodyBones.RightRingIntermediate;
            case NeuronBones.RightHandRing3:
                return HumanBodyBones.RightRingDistal;
            case NeuronBones.RightHandPinky1:
                return HumanBodyBones.RightLittleProximal;
            case NeuronBones.RightHandPinky2:
                return HumanBodyBones.RightLittleIntermediate;
            case NeuronBones.RightHandPinky3:
                return HumanBodyBones.RightLittleDistal;
            case NeuronBones.LeftShoulder:
                return HumanBodyBones.LeftShoulder;
            case NeuronBones.LeftArm:
                return HumanBodyBones.LeftUpperArm;
            case NeuronBones.LeftForeArm:
                return HumanBodyBones.LeftLowerArm;
            case NeuronBones.LeftHand:
                return HumanBodyBones.LeftHand;
            case NeuronBones.LeftHandThumb1:
                return HumanBodyBones.LeftThumbProximal;
            case NeuronBones.LeftHandThumb2:
                return HumanBodyBones.LeftThumbIntermediate;
            case NeuronBones.LeftHandThumb3:
                return HumanBodyBones.LeftThumbDistal;
            case NeuronBones.LeftHandIndex1:
                return HumanBodyBones.LeftIndexProximal;
            case NeuronBones.LeftHandIndex2:
                return HumanBodyBones.LeftIndexIntermediate;
            case NeuronBones.LeftHandIndex3:
                return HumanBodyBones.LeftIndexDistal;
            case NeuronBones.LeftHandMiddle1:
                return HumanBodyBones.LeftMiddleProximal;
            case NeuronBones.LeftHandMiddle2:
                return HumanBodyBones.LeftMiddleIntermediate;
            case NeuronBones.LeftHandMiddle3:
                return HumanBodyBones.LeftMiddleDistal;
            case NeuronBones.LeftHandRing1:
                return HumanBodyBones.LeftRingProximal;
            case NeuronBones.LeftHandRing2:
                return HumanBodyBones.LeftRingIntermediate;
            case NeuronBones.LeftHandRing3:
                return HumanBodyBones.LeftRingDistal;
            case NeuronBones.LeftHandPinky1:
                return HumanBodyBones.LeftLittleProximal;
            case NeuronBones.LeftHandPinky2:
                return HumanBodyBones.LeftLittleIntermediate;
            case NeuronBones.LeftHandPinky3:
                return HumanBodyBones.LeftLittleDistal;
            default:
                // in case no matching bone is found.
                return HumanBodyBones.Jaw;
        }
    }

    static void SetRotation(Animator animator, HumanBodyBones bone, Vector3 rotation, float lerp_ratio = 1)
    {
        Transform t = animator.GetBoneTransform(bone);
        if (t != null)
        {
            Quaternion rot = Quaternion.Slerp(t.localRotation, Quaternion.Euler(rotation), lerp_ratio);
            if (!float.IsNaN(rot.x) && !float.IsNaN(rot.y) && !float.IsNaN(rot.z) && !float.IsNaN(rot.w))
            {
                t.localRotation = rot;
            }
        }
    }

    static void SetPosition(Animator animator, HumanBodyBones bone, Vector3 position, float lerp_ratio = 1)
    {
        Transform t = animator.GetBoneTransform(bone);
        if (t != null)
        {
            // calculate position when we have scale
            position.Scale(new Vector3(1.0f / t.parent.lossyScale.x, 1.0f / t.parent.lossyScale.y, 1.0f / t.parent.lossyScale.z));

            Vector3 pos = Vector3.Lerp(t.localPosition, position, lerp_ratio);
            if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y) && !float.IsNaN(pos.z))
            {
// suit is bugged and position drifts off.
// t.localPosition = pos;
            }
        }
    }
}

