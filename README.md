# SlnLauncher

Kleiner Windows-Launcher, der `.sln`-Dateien automatisch mit der passenden
Anwendung öffnet – abhängig vom Inhalt der Solution-Datei. Damit lassen sich
z. B. normale Visual-Studio-Solutions von TwinCAT-XAE-Shell-Solutions
unterscheiden, ohne jedes Mal über das Kontextmenü "Öffnen mit" zu gehen.

## Funktionsweise

1. Windows ruft `SlnLauncher.exe "<Pfad-zur-sln>"` auf (registriert als
   Standard-Handler für `.sln`).
2. Der Launcher liest die ersten Zeilen der `.sln`-Datei und prüft sie gegen
   die in `config.json` definierten Marker (z. B. `TcXaeShell Solution File`).
3. Je nach Treffer wird die passende Ziel-Anwendung mit dem Solution-Pfad
   als Argument gestartet (z. B. `TcXaeShell.exe` oder `devenv.exe`).
4. Kein Treffer → `defaultTarget` aus der Config wird verwendet.

## Verzeichnisstruktur

```
C:\Tools\SlnLauncher\
  SlnLauncher.exe
  config.json
```

Beide Dateien müssen im selben Ordner liegen. Die exe liest `config.json`
relativ zu ihrem eigenen Speicherort (`AppContext.BaseDirectory`).

## config.json

```json
{
  "devenvPath": "C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\Common7\\IDE\\devenv.exe",
  "tcXaeShellPath": "C:\\Program Files (x86)\\Beckhoff\\TwinCAT\\3.1\\SystemManager\\TcXaeShell.exe",
  "markers": [
    { "contains": "TcXaeShell Solution File", "target": "tcXaeShellPath" }
  ],
  "defaultTarget": "devenvPath"
}
```

| Feld | Beschreibung |
|---|---|
| `devenvPath` | Pfad zu `devenv.exe` (Visual Studio 2022) |
| `tcXaeShellPath` | Pfad zu `TcXaeShell.exe` (TwinCAT XAE Shell) |
| `markers` | Liste von Regeln: `contains` = Suchstring in den ersten 5 Zeilen der `.sln`, `target` = Schlüssel der Ziel-Anwendung |
| `defaultTarget` | Ziel-Schlüssel, falls kein Marker zutrifft |

**Pfade anpassen und neue Regeln ergänzen geht direkt in dieser Datei –
kein Neukompilieren nötig.** Neue IDE-Varianten lassen sich durch weitere
Einträge in `markers` sowie einen zusätzlichen Pfad-Key ergänzen (dafür muss
der C#-Code aktuell noch um den neuen Key im `switch`-Ausdruck erweitert
werden).

## Build & Publish

Voraussetzung: [.NET 8 SDK](https://dotnet.microsoft.com/download).

```powershell
cd SlnLauncher
Remove-Item -Recurse -Force bin, obj -ErrorAction SilentlyContinue
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Ergebnis liegt unter:

```
SlnLauncher\bin\Release\net8.0-windows\win-x64\publish\
  SlnLauncher.exe
  config.json
```

Beide Dateien in den finalen Ordner kopieren, z. B. `C:\Tools\SlnLauncher\`.

## Als Standard-App für .sln registrieren

1. Rechtsklick auf eine beliebige `.sln`-Datei → **Öffnen mit** →
   **Andere App auswählen**
2. Ganz unten **Weitere Apps** → **Andere App auf diesem PC suchen**
3. Zu `SlnLauncher.exe` navigieren und auswählen
4. Häkchen bei **Immer diese App zum Öffnen von .sln-Dateien verwenden**
   setzen

Windows übergibt den Dateipfad automatisch als erstes Argument.


## Bekannte Einschränkungen

- Erkennung basiert auf reinem Text-Matching in den ersten Zeilen – keine
  vollständige Parsing-Logik der `.sln`-Struktur.
- Neue Ziel-Anwendungen (zusätzlich zu `devenv`/`TcXaeShell`) erfordern
  aktuell eine Code-Anpassung (neuer Key im `switch`), nicht nur eine
  Config-Änderung.
