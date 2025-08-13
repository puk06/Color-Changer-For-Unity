using System;
using UnityEditor;
using UnityEngine;

internal static class MaterialUtils
{
    /// <summary>
    /// �n���ꂽ�}�e���A���̑S�v���p�e�B�����[�v�ŉ񂵂܂��B
    /// Action�ɂ͌��̃e�N�X�`���Ƃ��̃v���p�e�B�����n����܂��B
    /// </summary>
    /// <param name="material"></param>
    /// <param name="action"></param>
    internal static void ForEachTex(Material material, Action<Texture, string> action)
    {
        Shader shader = material.shader;
        int propertyCount = GetPropertyCount(shader);

        for (int i = 0; i < propertyCount; i++)
        {
            if (!IsTexture(shader, i)) continue;

            string propName = ShaderUtil.GetPropertyName(shader, i);
            Texture materialTexture = material.GetTexture(propName);
            if (materialTexture == null) continue;

            action(materialTexture, propName);
        }
    }

    /// <summary>
    /// �n���ꂽ�}�e���A���̑S�v���p�e�B�����[�v�ŉ񂵁A�����P�ł��}�e���A���œn���ꂽ�e�N�X�`�����g���Ă�����true��Ԃ��܂��B
    /// </summary>
    /// <param name="material"></param>
    /// <param name="targetTexture"></param>
    /// <returns></returns>
    internal static bool AnyTex(Material material, Texture targetTexture)
    {
        Shader shader = material.shader;
        int propertyCount = GetPropertyCount(shader);

        for (int i = 0; i < propertyCount; i++)
        {
            if (!IsTexture(shader, i)) continue;

            string propertyName = ShaderUtil.GetPropertyName(shader, i);
            Texture materialTexture = material.GetTexture(propertyName);
            if (materialTexture == null) continue;

            if (materialTexture == targetTexture) return true;
        }

        return false;
    }

    /// <summary>
    /// �V�F�[�_�[�̃v���p�e�B�̐���Ԃ��܂��B
    /// </summary>
    /// <param name="shader"></param>
    /// <returns></returns>
    internal static int GetPropertyCount(Shader shader)
        => ShaderUtil.GetPropertyCount(shader);

    internal static bool IsTexture(Shader shader, int propertyIndex)
        => ShaderUtil.GetPropertyType(shader, propertyIndex) == ShaderUtil.ShaderPropertyType.TexEnv;
}
