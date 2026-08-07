import configparser
import re
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
LOCALES = ROOT / "locales"
LOCALE_SOURCE = ROOT / "Jammer.Core" / "src" / "Locale.cs"
PLACEHOLDER = re.compile(r"\{\d+(?:[^}]*)?\}")
LOCALE_LOOKUP = re.compile(
    r'CheckValueLocale\(\s*"(?P<section>[^"]+)"\s*,\s*"(?P<key>[^"]+)"'
)


def read_locale(path: Path) -> configparser.ConfigParser:
    parser = configparser.ConfigParser(interpolation=None)
    parser.optionxform = str
    with path.open("r", encoding="utf-8-sig") as locale_file:
        parser.read_file(locale_file)
    return parser


def entries(parser: configparser.ConfigParser) -> set[tuple[str, str]]:
    return {(section, key) for section in parser.sections() for key in parser[section]}


def validate_all_locales() -> bool:
    english = read_locale(LOCALES / "en.ini")
    english_entries = entries(english)
    errors: list[str] = []

    source = LOCALE_SOURCE.read_text(encoding="utf-8")
    source = re.sub(r"/\*.*?\*/", "", source, flags=re.DOTALL)
    source = re.sub(r"//.*", "", source)
    code_entries = {
        (match.group("section"), match.group("key"))
        for match in LOCALE_LOOKUP.finditer(source)
    }
    for section, key in sorted(code_entries - english_entries):
        errors.append(f"en.ini is missing [{section}] {key}, which Locale.cs uses")
    for section, key in sorted(english_entries - code_entries):
        errors.append(f"en.ini has unused entry [{section}] {key}")

    for path in sorted(LOCALES.glob("*.ini")):
        if path.name == "en.ini":
            continue
        locale = read_locale(path)
        locale_entries = entries(locale)
        for section, key in sorted(english_entries - locale_entries):
            errors.append(f"{path.name} is missing [{section}] {key}")
        for section, key in sorted(locale_entries - english_entries):
            errors.append(f"{path.name} has unexpected entry [{section}] {key}")
        for section, key in sorted(english_entries & locale_entries):
            expected = sorted(PLACEHOLDER.findall(english[section][key]))
            actual = sorted(PLACEHOLDER.findall(locale[section][key]))
            if actual != expected:
                errors.append(
                    f"{path.name} [{section}] {key} placeholders are {actual}; expected {expected}"
                )

        if not any(error.startswith(path.name) for error in errors):
            print(f"{path.name} is valid!")

    if errors:
        print("\n".join(errors), file=sys.stderr)
        return False
    return True


if __name__ == "__main__":
    raise SystemExit(0 if validate_all_locales() else 1)
