
const float theta = 270;

//�X�P�b�`�G�t�F�N�g�ł̒萔
const float noiseOffset = 0.5f;
const float noiseIntensity = 1.8f;
const float noiseCoefficient = 0.015f;
const float blurIntensity[2] = { 0.0015f, 0.001f };
const float percentage = 0.5625f;

//�X�P�b�`�G�t�F�N�g�{�F�����ł̒萔
const float sketchBlurIntensity = 0.002f;
const float fringeIntensity = 0.002f;
const float colorAdjustValue = 0.5f;

//���m�g�[���G�t�F�N�g�{�c�݃G�t�F�N�g�ł̒萔
const float curveIntensity = 0.1f;

//�Z�s�A�g�[���G�t�F�N�g�ł̒萔
const float4 sepiaTone = float4(0.8f, 0.45f, 0.1f, 1.0f);

//�c�݃G�t�F�N�g�ł̒萔
const float distortionOffset = -0.5f;
const float distortionIntensity = 2.0f;
const float distortionCoefficient = 0.03f;