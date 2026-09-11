# Sonarae

Beautiful terminal lyrics utility.

Sonarae displays synchronized song lyrics directly in your terminal with smooth animated rendering and multiple visual styles.

## Usage

```bash
sonarae
```

Select a style:

```bash
sonarae --style karaoke
sonarae --style focus
```

Control the number of surrounding lyric lines:

```bash
sonarae --style karaoke --lines 3
```

Show help:

```bash
sonarae --help
```

## Styles

| Style | Description |
|---|---|
| `karaoke` | Animated gradient lyrics |
| `focus` | One word at a time with word sync |

## Requirements

- Linux
- A terminal with truecolor support
- An MPRIS-compatible music player
- .NET 10