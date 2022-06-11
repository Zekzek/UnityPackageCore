using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Zekzek.DebugConsole
{
    public class DebugConsoleBehaviour : MonoBehaviour
    {
        [SerializeField] private GameObject _wrapper;
        [SerializeField] private ScrollRect _scroller;
        [SerializeField] private Text _history;
        [SerializeField] private InputField _input;

        private bool _active;
        private List<string> commandHistory = new List<string>();
        private int commandIndex = 0;
        private string inProgressCommand;
        private Dictionary<string, AbstractConsoleCommand> registeredCommands = new Dictionary<string, AbstractConsoleCommand>();

        public void Toggle()
        {
            if (_active) { Hide(); } else { Show(); }
        }

        public void Show()
        {
            _active = true;
            _wrapper.SetActive(true);
            _input.onEndEdit.RemoveListener(EndEdit);
            _input.onEndEdit.AddListener(EndEdit);
            _input.ActivateInputField();

            Debug.Log("Deactivated Input Field");
        }

        public void Hide()
        {
            _active = false;
            _wrapper.SetActive(false);
            _input.onEndEdit.RemoveListener(EndEdit);
        }

        public void Register(AbstractConsoleCommand command)
        {
            foreach (string alias in command.Aliases) {
                if (registeredCommands.ContainsKey(alias.ToLower())) {
                    Debug.LogError("Unable to register duplicate command: " + alias);
                } else {
                    registeredCommands.Add(alias.ToLower(), command);
                }
            }
        }

        private void Awake()
        {
            if (UnityEngine.EventSystems.EventSystem.current == null) {
                Debug.LogWarning("No EventSystem found. Generating one now.");
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            // Register new commands here
            // Register(new InfoConsoleCommand());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote)) { Toggle(); }
            if (_active) {
                if (Input.GetKeyDown(KeyCode.UpArrow)) { ShowLastCommand(); }
                if (Input.GetKeyDown(KeyCode.DownArrow)) { ShowNextCommand(); }
            }
        }

        private void ShowLastCommand()
        {
            if (commandIndex == -1) {
                inProgressCommand = _input.text;
            }
            if (commandIndex < commandHistory.Count - 1) {
                commandIndex++;
                _input.text = commandHistory[commandIndex];
            }
        }

        private void ShowNextCommand()
        {
            if (commandIndex > 0) {
                commandIndex--;
                _input.text = commandHistory[commandIndex];
            } else {
                commandIndex = -1;
                _input.text = inProgressCommand;
            }
        }

        private void EndEdit(string input)
        {
            if (Input.GetButton("Submit") && input != null) {
                input = input.Trim();
                Write(" > " + input);
                if (!string.IsNullOrEmpty(input)) {
                    commandHistory.Insert(0, input);
                    commandIndex = -1;
                    if (Input.GetButton("Submit")) {
                        Process(input);
                    }
                    _input.text = "";
                }
            }
            _input.ActivateInputField();
        }

        private void Process(string value)
        {
            List<string> splitCommand = new List<string>(value.Split(' '));

            string commandName = splitCommand[0].ToLower();
            splitCommand.RemoveAt(0);

            if ("?".Equals(commandName)) {
                ProcessHelp(splitCommand);
            } else if (registeredCommands.ContainsKey(commandName)) {
                registeredCommands[commandName].Process(Write, splitCommand);
            } else {
                Write("No command found for: " + commandName);
            }
        }

        private void ProcessHelp(List<string> splitCommand)
        {
            if (splitCommand.Count == 0) {
                Write("Available commands:");
                HashSet<AbstractConsoleCommand> uniqueCommands = new HashSet<AbstractConsoleCommand>(registeredCommands.Values);
                Write($"[?] [<command name>]?\tShow help text for available commands");
                foreach (var command in uniqueCommands) {
                    Write($"[{string.Join(", ", command.Aliases).ToUpper()}]\t{command.Description}");
                }
                return;
            }

            string commandName = splitCommand[0].ToLower();
            if (registeredCommands.ContainsKey(commandName)) {
                var command = registeredCommands[commandName];
                Write($"{string.Join(", ", command.Aliases).ToUpper()}");
                Write(command.Description);
                Write(command.HelpText);
            } else {
                Write("No command found for: " + commandName);
            }
        }

        private void Write(string line)
        {
            _history.text += line + "\n";
            _scroller.verticalNormalizedPosition = 0;
        }
    }
}