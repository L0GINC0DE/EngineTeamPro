using _01.Scripts.TAE.Players;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01.Scripts.TAE.Managers
{
    public sealed class LabStage : MonoBehaviour
    {
        private enum StageState { Briefing, Playing, Failed, Cleared }

        [SerializeField] private PlatformPlayer player;
        [SerializeField] private Transform checkpoint;
        [SerializeField] private Transform exit;
        [SerializeField] private float fallLimit = -9f;

        private static readonly Color Accent = new(0.34f, 0.91f, 0.82f);
        private StageState state;
        private Vector2 entry;
        private Vector2 respawn;
        private Rigidbody2D body;
        private Camera view;
        private float elapsed;
        private int attempts;
        private bool reachedCheckpoint;
        private GUIStyle heading;
        private GUIStyle text;
        private GUIStyle action;

        private void Start()
        {
            body = player.GetComponent<Rigidbody2D>();
            view = Camera.main;
            entry = respawn = body.position;
            Pause(StageState.Briefing);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            bool confirm = keyboard != null &&
                (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame);

            if (state != StageState.Playing)
            {
                if (confirm)
                    Continue();
                return;
            }

            if (Time.timeScale < 0.1f)
                return;

            if (body.position.y < fallLimit || player.HealthCompo.IsDead)
            {
                Pause(StageState.Failed);
                return;
            }

            if (keyboard?.rKey.wasPressedThisFrame == true)
            {
                Continue();
                return;
            }

            elapsed += Time.deltaTime;
            Vector2 position = body.position;
            if (!reachedCheckpoint && position.x >= checkpoint.position.x &&
                position.y >= checkpoint.position.y - 0.2f)
            {
                reachedCheckpoint = true;
                respawn = (Vector2)checkpoint.position + new Vector2(-0.8f, 0.45f);
            }

            Vector2 toExit = position - (Vector2)exit.position;
            if (Mathf.Abs(toExit.x) < 0.62f && toExit.y >= -0.25f && toExit.y < 0.95f &&
                Mathf.Abs(body.linearVelocityY) < 0.15f)
                Pause(StageState.Cleared);
        }

        private void Pause(StageState next)
        {
            state = next;
            player.MoveCompo.StopInput(true);
            Time.timeScale = 0f;
        }

        private void Continue()
        {
            if (state == StageState.Briefing || state == StageState.Cleared)
            {
                reachedCheckpoint = false;
                respawn = entry;
                elapsed = 0f;
                attempts = 0;
            }

            GimmickManager.Instance.ResetActivePoint();
            player.HealthCompo.ResetHealth();
            player.MoveCompo.ResetForStage(respawn);
            state = StageState.Playing;
            attempts++;
            Time.timeScale = 1f;
        }

        private void OnDestroy()
        {
            if (body != null && state != StageState.Playing)
                Time.timeScale = 1f;
        }

        private void OnGUI()
        {
            if (body == null)
                return;

            if (heading == null)
            {
                heading = new GUIStyle(GUI.skin.label) { fontSize = 31, fontStyle = FontStyle.Bold };
                heading.normal.textColor = Color.white;
                text = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
                text.normal.textColor = Color.white;
                action = new GUIStyle(GUI.skin.button) { fontSize = 17, fontStyle = FontStyle.Bold };
            }

            float scale = Mathf.Min(Screen.width / 960f, Screen.height / 540f);
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
            float width = Screen.width / scale;
            float height = Screen.height / scale;

            if (state == StageState.Playing)
            {
                DrawHud(width);
                DrawExitMarker(scale, width, height);
            }
            else
                DrawOverlay(width, height);

            GUI.matrix = previous;
        }

        private void DrawHud(float width)
        {
            Fill(new Rect(24, 20, 232, 72), new Color(0.03f, 0.08f, 0.14f, 0.93f));
            var health = player.HealthCompo;
            GUI.Label(new Rect(38, 28, 200, 22), $"HP  {health.CurrentHealth} / {health.MaxHealth}", text);
            Fill(new Rect(38, 52, 204, 6), new Color(0.16f, 0.27f, 0.33f));
            Fill(new Rect(38, 52, 204f * health.CurrentHealth / health.MaxHealth, 6), Accent);
            GUI.Label(new Rect(38, 63, 204, 22), reachedCheckpoint ? "CHECKPOINT SAVED" : "REACH THE UPPER DECK", text);

            Fill(new Rect(width - 212, 20, 188, 54), new Color(0.03f, 0.08f, 0.14f, 0.93f));
            GUI.Label(new Rect(width - 198, 27, 165, 22), $"LAB 01   {elapsed:0.0}s", text);
            GUI.Label(new Rect(width - 198, 48, 165, 22), "C  Tools     R  Retry", text);
        }

        private void DrawExitMarker(float scale, float width, float height)
        {
            if (view == null)
                return;

            Vector3 point = view.WorldToScreenPoint(exit.position + Vector3.up * 1.25f);
            float x = point.x / scale;
            float y = height - point.y / scale;
            if (point.z <= 0f || x < 48f || x > width - 48f || y < 18f || y > height - 18f)
                return;

            Fill(new Rect(x - 39, y - 12, 78, 24), new Color(0.04f, 0.16f, 0.19f, 0.96f));
            GUI.Label(new Rect(x - 24, y - 11, 52, 22), "EXIT", text);
        }

        private void DrawOverlay(float width, float height)
        {
            Fill(new Rect(0, 0, width, height), new Color(0.015f, 0.04f, 0.08f, 0.66f));
            var panel = new Rect((width - 488) / 2f, (height - 252) / 2f, 488, 252);
            Fill(panel, new Color(0.035f, 0.08f, 0.14f, 0.99f));
            Fill(new Rect(panel.x, panel.y, panel.width, 3), Accent);

            string title = "Interface Lab";
            string description = "Reach the exit on the upper deck.\n\nA / D  Move     Space  Jump\nC  Select point     Arrows  Choose     Enter  Confirm";
            string button = "Start run";
            if (state == StageState.Failed)
            {
                title = "Run interrupted";
                description = reachedCheckpoint ? "Return to the far-side checkpoint." : "Return to the entrance.";
                button = "Retry";
            }
            else if (state == StageState.Cleared)
            {
                title = "Stage cleared";
                description = $"Clear time  {elapsed:0.0}s\nAttempts  {attempts}";
                button = "Play again";
            }

            GUI.Label(new Rect(panel.x + 30, panel.y + 27, 428, 48), title, heading);
            GUI.Label(new Rect(panel.x + 30, panel.y + 86, 428, 104), description, text);
            if (GUI.Button(new Rect(panel.x + 30, panel.y + 194, 428, 36), button, action))
                Continue();
        }

        private static void Fill(Rect area, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
