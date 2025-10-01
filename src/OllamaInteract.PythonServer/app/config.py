import json

class AppConfig:
    def __init__(self):
        self.python_server_host = "http://localhost"
        self.python_server_port = 8000
        self.ollama_host = "http://localhost"
        self.ollama_port = 11434


    def _load_from_file(self):
        config_path = self._find_config_file()

        if not config_path or not os.path.exists(config_path):
            print(f"Config file not found, using defaults")
            return;

        try:
            with open(config_path, "r") as file:
                data = json.load(file)
                
            for key, value in data.items():
                if hasattr(self, key):
                    setattr(self, key, value)
        
            print(f"Config loaded from: {config_path}")
        except Exception as e:
            print(f"Error loading config: {e}")

    def _find_config_file():
        if os.name == "nt":
            app_data = os.getenv("APPDATA")
            if app_data:
                return os.path.join(app_data, "OllamaInteract", "config.json")
        elif os.name == "posix":
            config_home = os.getenv("XDG_CONFIG_HOME") or (
                os.path.expanduser("~/Library/Application Support") if os.username().sysname == "Darwin"
                else os.path.expanduser("~/.config")
            )
            return os.path.join(config_home, "OllamaInteract", "config.json")
        
        return None