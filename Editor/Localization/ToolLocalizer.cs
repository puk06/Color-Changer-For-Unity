using System.Collections.Generic;

namespace net.puk06.ColorChanger.Localization
{
    public static class ToolLocalizer
    {
        private static readonly Dictionary<string, Dictionary<string, string>> _localizationDictionary = new()
        {
            ["editorwindow.childObject.warning"] = new()
            {
                ["ja"] = "このオブジェクトはアバターの子オブジェクトとして配置する必要があります。",
                ["en"] = "This object must be placed as a child of the avatar.",
                ["ko"] = "이 오브젝트는 아바타의 자식 오브젝트로 배치해야 합니다."
            },
            ["editorwindow.scriptsetting"] = new()
            {
                ["ja"] = "スクリプト設定",
                ["en"] = "Script Settings",
                ["ko"] = "스크립트 설정"
            },
            ["editorwindow.scriptsetting.enable"] = new()
            {
                ["ja"] = "スクリプトの有効化",
                ["en"] = "Enable Script",
                ["ko"] = "스크립트 활성화"
            },
            ["editorwindow.scriptsetting.previewenable"] = new()
            {
                ["ja"] = "プレビューの有効化",
                ["en"] = "Enable Preview",
                ["ko"] = "프리뷰 활성화"
            },
            ["editorwindow.scriptsetting.cpurendering.warning"] = new()
            {
                ["ja"] = "CPUレンダリングは、GPUがプレビューに対応していなかったときのみ使用してください。\nCPUプレビューは毎回プレビューを作成するのに時間がかかります。扱いには注意してください。",
                ["en"] = "Use CPU rendering only when the GPU does not support preview.\nCPU preview takes time to generate each time. Please use with caution.",
                ["ko"] = "CPU 렌더링은 GPU가 프리뷰를 지원하지 않을 때만 사용하세요.\nCPU 프리뷰는 매번 생성하는 데 시간이 오래 걸립니다. 주의해서 사용하세요."
            },
            ["editorwindow.scriptsetting.cpurendering.enable"] = new()
            {
                ["ja"] = "CPUレンダリングの有効化",
                ["en"] = "Enable CPU Rendering",
                ["ko"] = "CPU 렌더링 활성화"
            },
            ["editorwindow.scriptsetting.mlic.info"] = new()
            {
                ["ja"] = "TexTransToolのMultiLayerImageCanvasが親オブジェクトにあります。\nExternalToolAsLayerコンポーネントを追加することで、MultiLayerImageCanvasのレイヤーとして扱うことができます。",
                ["en"] = "MultiLayerImageCanvas of TexTransTool exists in the parent object.\nBy adding the ExternalToolAsLayer component, you can treat it as a layer of MultiLayerImageCanvas.",
                ["ko"] = "TexTransTool의 MultiLayerImageCanvas가 부모 오브젝트에 있습니다.\nExternalToolAsLayer 컴포넌트를 추가하면 MultiLayerImageCanvas의 레이어로 취급할 수 있습니다."
            },
            ["editorwindow.scriptsetting.mlic.add"] = new()
            {
                ["ja"] = "ExternalToolAsLayerコンポーネントを追加する",
                ["en"] = "Add ExternalToolAsLayer Component",
                ["ko"] = "ExternalToolAsLayer 컴포넌트 추가"
            },
            ["editorwindow.scriptsetting.mlic.warning"] = new()
            {
                ["ja"] = "TexTransToolのMultiLayerImageCanvasが親オブジェクトにありません。\n通常通り動作するにはExternalToolAsLayerコンポーネントを外す必要があります。",
                ["en"] = "MultiLayerImageCanvas of TexTransTool does not exist in the parent object.\nTo operate normally, you need to remove the ExternalToolAsLayer component.",
                ["ko"] = "TexTransTool의 MultiLayerImageCanvas가 부모 오브젝트에 없습니다.\n정상적으로 동작하려면 ExternalToolAsLayer 컴포넌트를 제거해야 합니다."
            },
            ["editorwindow.scriptsetting.mlic.remove"] = new()
            {
                ["ja"] = "ExternalToolAsLayerコンポーネントを削除する",
                ["en"] = "Remove ExternalToolAsLayer Component",
                ["ko"] = "ExternalToolAsLayer 컴포넌트 제거"
            },
            ["editorwindow.texturesetting"] = new()
            {
                ["ja"] = "テクスチャ設定",
                ["en"] = "Texture Settings",
                ["ko"] = "텍스처 설정"
            },
            ["editorwindow.texturesetting.target"] = new()
            {
                ["ja"] = "適用したいテクスチャ",
                ["en"] = "Texture to Apply",
                ["ko"] = "적용할 텍스처"
            },
            ["editorwindow.colorsetting"] = new()
            {
                ["ja"] = "色設定",
                ["en"] = "Color Settings",
                ["ko"] = "색상 설정"
            },
            ["editorwindow.colorsetting.previouscolor"] = new()
            {
                ["ja"] = "変更前の色",
                ["en"] = "Previous Color",
                ["ko"] = "변경 전 색상"
            },
            ["editorwindow.colorsetting.newcolor"] = new()
            {
                ["ja"] = "変更後の色",
                ["en"] = "New Color",
                ["ko"] = "변경 후 색상"
            },
            ["editorwindow.balancemode"] = new()
            {
                ["ja"] = "バランスモード",
                ["en"] = "Balance Mode",
                ["ko"] = "밸런스 모드"
            },
            ["editorwindow.balancemode.description"] = new()
            {
                ["ja"] = "色変更の計算式を、テクスチャ改変に適した形式に切り替えます。",
                ["en"] = "Switches the color change calculation formula to a format suitable for texture modification.",
                ["ko"] = "색상 변경 계산식을 텍스처 변형에 적합한 형식으로 전환합니다."
            },
            ["editorwindow.balancemode.setting"] = new()
            {
                ["ja"] = "バランスモード設定",
                ["en"] = "Balance Mode Settings",
                ["ko"] = "밸런스 모드 설정"
            },
            ["editorwindow.balancemode.v1"] = new()
            {
                ["ja"] = "バランスモードV1",
                ["en"] = "Balance Mode V1",
                ["ko"] = "밸런스 모드 V1"
            },
            ["editorwindow.balancemode.v1.description"] = new()
            {
                ["ja"] = "選んだ色と各ピクセルの色の距離、およびその延長線上の位置から変化率を計算します。\nデメリット: RGB空間の端に近い色は変化が小さくなります。",
                ["en"] = "Calculates the rate of change based on the distance between the selected color and each pixel color, and the position on the extension line.\nDisadvantage: Colors near the edge of the RGB space change less.",
                ["ko"] = "선택한 색상과 각 픽셀의 색상 거리, 그리고 그 연장선상의 위치에서 변화율을 계산합니다.\n단점: RGB 공간의 끝에 가까운 색상은 변화가 적습니다."
            },
            ["editorwindow.balancemode.v1.weight"] = new()
            {
                ["ja"] = "変化率グラフの重み",
                ["en"] = "Weight of Change Rate Graph",
                ["ko"] = "변화율 그래프의 가중치"
            },
            ["editorwindow.balancemode.v1.minvalue"] = new()
            {
                ["ja"] = "変化率グラフの最低値",
                ["en"] = "Minimum Value of Change Rate Graph",
                ["ko"] = "변화율 그래프의 최소값"
            },
            ["editorwindow.balancemode.v2"] = new()
            {
                ["ja"] = "バランスモードV2",
                ["en"] = "Balance Mode V2",
                ["ko"] = "밸런스 모드 V2"
            },
            ["editorwindow.balancemode.v2.description"] = new()
            {
                ["ja"] = "選んだ色を中心に球状に色の変化率を計算し、半径の位置を基準とします。\nデメリット: RGB空間の制限を受けませんが、設定が少し複雑です。",
                ["en"] = "Calculates the rate of color change in a spherical shape centered on the selected color, based on the radius position.\nDisadvantage: Not limited by RGB space, but settings are a bit complex.",
                ["ko"] = "선택한 색상을 중심으로 구형으로 색상 변화율을 계산하며, 반지름 위치를 기준으로 합니다.\n단점: RGB 공간의 제한을 받지 않지만 설정이 다소 복잡합니다."
            },
            ["editorwindow.balancemode.v2.radius"] = new()
            {
                ["ja"] = "球の半径の最大値",
                ["en"] = "Maximum Sphere Radius",
                ["ko"] = "구의 최대 반지름"
            },
            ["editorwindow.balancemode.v2.weight"] = new()
            {
                ["ja"] = "変化率グラフの重み",
                ["en"] = "Weight of Change Rate Graph",
                ["ko"] = "변화율 그래프의 가중치"
            },
            ["editorwindow.balancemode.v2.minvalue"] = new()
            {
                ["ja"] = "変化率グラフの最低値",
                ["en"] = "Minimum Value of Change Rate Graph",
                ["ko"] = "변화율 그래프의 최소값"
            },
            ["editorwindow.balancemode.v2.includeoutside"] = new()
            {
                ["ja"] = "範囲外にも最低値を適用する",
                ["en"] = "Apply Minimum Value Outside Range",
                ["ko"] = "범위 밖에도 최소값 적용"
            },
            ["editorwindow.balancemode.v3"] = new()
            {
                ["ja"] = "バランスモードV3",
                ["en"] = "Balance Mode V3",
                ["ko"] = "밸런스 모드 V3"
            },
            ["editorwindow.balancemode.v3.weight"] = new()
            {
                ["ja"] = "変化率グラフの重み",
                ["en"] = "Weight of Change Rate Graph",
                ["ko"] = "변화율 그래프의 가중치"
            },
            ["editorwindow.balancemode.v3.description"] = new()
            {
                ["ja"] = "設定されたグラデーションに沿って、ピクセルの明るさから変化率を決めます。\nデメリット: 色が均一に変わりますが、意図しない部分も変化する可能性があります。",
                ["en"] = "Determines the rate of change from pixel brightness along the set gradient.\nDisadvantage: Colors change uniformly, but unintended areas may also change.",
                ["ko"] = "설정된 그라데이션을 따라 픽셀 밝기에서 변화율을 결정합니다.\n단점: 색상이 균일하게 변하지만, 의도하지 않은 부분도 변할 수 있습니다."
            },
            ["editorwindow.balancemode.v3.gradient"] = new()
            {
                ["ja"] = "グラデーション",
                ["en"] = "Gradient",
                ["ko"] = "그라데이션"
            },
            ["editorwindow.balancemode.v3.previewresolution"] = new()
            {
                ["ja"] = "プレビュー解像度",
                ["en"] = "Preview Resolution",
                ["ko"] = "프리뷰 해상도"
            },
            ["editorwindow.advancedsettings"] = new()
            {
                ["ja"] = "色の追加設定",
                ["en"] = "Additional Color Settings",
                ["ko"] = "추가 색상 설정"
            },
            ["editorwindow.advancedsettings.enable"] = new()
            {
                ["ja"] = "有効",
                ["en"] = "Enable",
                ["ko"] = "활성화"
            },
            ["editorwindow.advancedsettings.brightness"] = new()
            {
                ["ja"] = "明るさ",
                ["en"] = "Brightness",
                ["ko"] = "밝기"
            },
            ["editorwindow.advancedsettings.contrast"] = new()
            {
                ["ja"] = "コントラスト",
                ["en"] = "Contrast",
                ["ko"] = "명암"
            },
            ["editorwindow.advancedsettings.gamma"] = new()
            {
                ["ja"] = "ガンマ",
                ["en"] = "Gamma",
                ["ko"] = "감마"
            },
            ["editorwindow.advancedsettings.exposure"] = new()
            {
                ["ja"] = "露出",
                ["en"] = "Exposure",
                ["ko"] = "노출"
            },
            ["editorwindow.advancedsettings.transparency"] = new()
            {
                ["ja"] = "透明度",
                ["en"] = "Transparency",
                ["ko"] = "투명도"
            },
            ["editorwindow.textureoutput.warning"] = new()
            {
                ["ja"] = "テクスチャはビルド時に自動で非破壊で作成、適用されます。\nテクスチャ画像の細かな修正が必要な場合はテクスチャを出力して各自で編集してください。",
                ["en"] = "Textures are automatically created and applied non-destructively at build time.\nIf you need to make detailed corrections to the texture image, please export and edit it yourself.",
                ["ko"] = "텍스처는 빌드 시 자동으로 비파괴적으로 생성 및 적용됩니다.\n텍스처 이미지를 세밀하게 수정해야 하는 경우 텍스처를 출력하여 직접 편집하세요."
            },
            ["editorwindow.textureoutput.button"] = new()
            {
                ["ja"] = "テクスチャ出力(非推奨)",
                ["en"] = "Export Texture (Not Recommended)",
                ["ko"] = "텍스처 출력(비추천)"
            },
            ["editorwindow.generatetexture.missingtexture"] = new()
            {
                ["ja"] = "ターゲットテクスチャが選択されていません。",
                ["en"] = "Target texture is not selected.",
                ["ko"] = "타겟 텍스처가 선택되지 않았습니다."
            },
            ["editorwindow.generatetexture.success"] = new()
            {
                ["ja"] = "テクスチャの作成が完了しました。\nこのテクスチャを使用しているマテリアルを、現在のシーン内で更新しますか？",
                ["en"] = "Texture creation is complete.\nWould you like to update the materials using this texture in the current scene?",
                ["ko"] = "텍스처 생성이 완료되었습니다.\n이 텍스처를 사용하는 머티리얼을 현재 씬에서 업데이트하시겠습니까?"
            },
            ["editorwindow.generatetexture.failed"] = new()
            {
                ["ja"] = "テクスチャ処理に失敗しました: '{0}'\n{1}",
                ["en"] = "Texture processing failed: '{0}'\n{1}",
                ["ko"] = "텍스처 처리에 실패했습니다: '{0}'\n{1}"
            },
            ["editorwindow.generatetexture.success.confirm"] = new()
            {
                ["ja"] = "確認",
                ["en"] = "Confirm",
                ["ko"] = "확인"
            },
            ["editorwindow.generatetexture.success.yes"] = new()
            {
                ["ja"] = "はい",
                ["en"] = "Yes",
                ["ko"] = "예"
            },
            ["editorwindow.generatetexture.success.no"] = new()
            {
                ["ja"] = "いいえ",
                ["en"] = "No",
                ["ko"] = "아니오"
            },
            ["editorwindow.generatetexture.save.missingpath"] = new()
            {
                ["ja"] = "元テクスチャのパスが取得できません",
                ["en"] = "Cannot get the path of the original texture.",
                ["ko"] = "원본 텍스처의 경로를 가져올 수 없습니다."
            },
            ["editorwindow.generatetexture.save.encodefailed"] = new()
            {
                ["ja"] = "PNGデータのエンコードに失敗しました",
                ["en"] = "Failed to encode PNG data.",
                ["ko"] = "PNG 데이터 인코딩에 실패했습니다."
            },
            ["editorwindow.generatetexture.save.success"] = new()
            {
                ["ja"] = "テクスチャを保存しました: {0}",
                ["en"] = "Texture saved: {0}",
                ["ko"] = "텍스처를 저장했습니다: {0}"
            },
            ["editorwindow.componentmanager.avatar"] = new()
            {
                ["ja"] = "アバター",
                ["en"] = "Avatar",
                ["ko"] = "아바타"
            },
            ["editorwindow.componentmanager.texture"] = new()
            {
                ["ja"] = "テクスチャ: {0} ({1})",
                ["en"] = "Texture: {0} ({1})",
                ["ko"] = "텍스처: {0} ({1})"
            },
            ["editorwindow.componentmanager.enabledcomponents"] = new()
            {
                ["ja"] = "有効なコンポーネント ({0}) ",
                ["en"] = "Enabled Components ({0}) ",
                ["ko"] = "활성화된 컴포넌트 ({0}) "
            },
            ["editorwindow.componentmanager.disabledcomponents"] = new()
            {
                ["ja"] = "無効なコンポーネント ({0}) ",
                ["en"] = "Disabled Components ({0}) ",
                ["ko"] = "비활성화된 컴포넌트 ({0}) "
            },
            ["editorwindow.componentmanager.texturemissing"] = new()
            {
                ["ja"] = "未割り当て",
                ["en"] = "Unassigned",
                ["ko"] = "미할당"
            },
        };

        internal static Dictionary<string, Dictionary<string, string>> LocalizationDictionary = _localizationDictionary;
    }
}
