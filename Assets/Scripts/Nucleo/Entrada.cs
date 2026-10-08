using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RumoAoHexa
{
    /// <summary>
    /// Leitura dos controles (teclado e gamepad) usando o Input System.
    /// O projeto está configurado com "Active Input Handling = Input System Package".
    /// </summary>
    public static class Entrada
    {
        /// <summary>-1 (esquerda) a 1 (direita).</summary>
        public static float Horizontal
        {
            get
            {
                float v = 0f;
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                if (kb != null)
                {
                    if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) v -= 1f;
                    if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) v += 1f;
                }
                Gamepad gp = Gamepad.current;
                if (gp != null)
                {
                    float x = gp.leftStick.ReadValue().x;
                    if (Mathf.Abs(x) > 0.35f) v += Mathf.Sign(x);
                    if (gp.dpad.left.isPressed) v -= 1f;
                    if (gp.dpad.right.isPressed) v += 1f;
                }
#else
                v = Input.GetAxisRaw("Horizontal");
#endif
                return Mathf.Clamp(v, -1f, 1f);
            }
        }

        /// <summary>↑ ou W segurado (mira para cima: o chute vira cavadinha).</summary>
        public static bool CimaSegurado
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad gp = Gamepad.current;
                return (kb != null && (kb.wKey.isPressed || kb.upArrowKey.isPressed)) ||
                       (gp != null && (gp.dpad.up.isPressed || gp.leftStick.ReadValue().y > 0.6f));
#else
                return Input.GetAxisRaw("Vertical") > 0.5f;
#endif
            }
        }

        public static bool BaixoSegurado
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad gp = Gamepad.current;
                return (kb != null && (kb.sKey.isPressed || kb.downArrowKey.isPressed)) ||
                       (gp != null && (gp.dpad.down.isPressed || gp.leftStick.ReadValue().y < -0.6f));
#else
                return Input.GetAxisRaw("Vertical") < -0.5f;
#endif
            }
        }

        /// <summary>Espaço ou Z (↑ fica livre para mirar a cavadinha).</summary>
        public static bool PuloApertado
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad gp = Gamepad.current;
                return (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.zKey.wasPressedThisFrame)) ||
                       (gp != null && gp.buttonSouth.wasPressedThisFrame);
#else
                return Input.GetButtonDown("Jump");
#endif
            }
        }

        public static bool PuloSegurado
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad gp = Gamepad.current;
                return (kb != null && (kb.spaceKey.isPressed || kb.zKey.isPressed)) ||
                       (gp != null && gp.buttonSouth.isPressed);
#else
                return Input.GetButton("Jump");
#endif
            }
        }

        /// <summary>J ou X: chute (segurar carrega a folha seca).</summary>
        public static bool ChuteApertado
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad gp = Gamepad.current;
                return (kb != null && (kb.jKey.wasPressedThisFrame || kb.xKey.wasPressedThisFrame)) ||
                       (gp != null && gp.buttonWest.wasPressedThisFrame);
#else
                return Input.GetButtonDown("Fire1");
#endif
            }
        }

        public static bool ChuteSegurado
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad gp = Gamepad.current;
                return (kb != null && (kb.jKey.isPressed || kb.xKey.isPressed)) ||
                       (gp != null && gp.buttonWest.isPressed);
#else
                return Input.GetButton("Fire1");
#endif
            }
        }

        /// <summary>K ou C: cavadinha direto (atalho para ↑ + J).</summary>
        public static bool CavadinhaApertada
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad gp = Gamepad.current;
                return (kb != null && (kb.kKey.wasPressedThisFrame || kb.cKey.wasPressedThisFrame)) ||
                       (gp != null && gp.buttonNorth.wasPressedThisFrame);
#else
                return Input.GetButtonDown("Fire2");
#endif
            }
        }

        public static bool PausaApertada
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad gp = Gamepad.current;
                return (kb != null && (kb.escapeKey.wasPressedThisFrame || kb.pKey.wasPressedThisFrame)) ||
                       (gp != null && gp.startButton.wasPressedThisFrame);
#else
                return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
#endif
            }
        }

        public static bool ConfirmarApertado
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad gp = Gamepad.current;
                return (kb != null && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)) ||
                       (gp != null && gp.startButton.wasPressedThisFrame);
#else
                return Input.GetKeyDown(KeyCode.Return);
#endif
            }
        }

        public static bool VoltarApertado
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad gp = Gamepad.current;
                return (kb != null && (kb.escapeKey.wasPressedThisFrame || kb.backspaceKey.wasPressedThisFrame)) ||
                       (gp != null && gp.buttonEast.wasPressedThisFrame);
#else
                return Input.GetKeyDown(KeyCode.Escape);
#endif
            }
        }

        /// <summary>Liga/desliga o som (tecla M).</summary>
        public static bool MudoApertado
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                return kb != null && kb.mKey.wasPressedThisFrame;
#else
                return Input.GetKeyDown(KeyCode.M);
#endif
            }
        }
    }
}
