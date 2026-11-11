# Funkcje wbudowane PolSkrypt

Wersja dokumentu: 0.4.0

Część referencji języka PolSkrypt (zob. [language-reference.md](language-reference.md)).

Funkcje wbudowane są dostępne globalnie, bez żadnych deklaracji. Metody natywne list (`dodaj`, `długość` itd.) są opisane osobno w [lists.md](lists.md).

## Zestawienie

| Funkcja | Opis | Przykład | Wynik |
|---------|------|----------|-------|
| `wypisz(x, ...)` | wypisuje wartości oddzielone spacją; **nie** dodaje na końcu znaku nowej linii; obsługuje sekwencje specjalne `\n`, `\b`, `\r` | `wypisz(1, "a");` | `1 a` |
| `wprowadź()` | pozwala na wprowadzenie wartości zmiennej z klawiatury w postaci napisu | `zmienna a = wprowadź();` | |
| `zegar()` | zwraca obecną godzinę UTC w sekundach w postaci liczby | `wypisz(zegar());` | `1793629096` |
| `liczba(x)` | zamienia wartość `x` na liczbę; zwraca błąd w przypadku niepoprawnego argumentu | `liczba("42");` | `42` |
| `napis(x)` | zamienia wartość `x` na napis | `napis(42);` | `"42"` |
| `typ(x)` | zwraca typ wartości `x` | `wypisz(typ(5));` | `LICZBA` |

## `wypisz`

Wypisuje podane wartości na standardowe wyjście. Wartości są oddzielone spacją. Od wersji 0.4.0 wywołanie z **jednym** argumentem nie dodaje już żadnego dodatkowego znaku (wcześniej wypisywało po nim zbędną spację).

`wypisz` nie kończy wypisanego tekstu znakiem nowej linii. Żeby przejść do nowej linii, użyj sekwencji `\n`:

```text
wypisz("a");            // wypisze: a
wypisz("b");            // wypisze: b (w tej samej linii, razem: ab)
wypisz("c\n");          // wypisze: c i przejdzie do nowej linii
wypisz(1, "a", prawda); // wypisze: 1 a prawda
wypisz([1, 2, 3]);      // wypisze: [1, 2, 3]
```

Liczby całkowite są wypisywane bez części dziesiętnej (`wypisz(3.0)` wypisze `3`), a wartości logiczne jako `prawda` i `fałsz`.

## `wprowadź`

Pobiera wartość wprowadzoną z klawiatury i zwraca ją jako **napis**. Aby użyć jej jako liczby, przekonwertuj ją funkcją `liczba`:

```text
wypisz("Podaj liczbę: ");
zmienna n = liczba(wprowadź());
wypisz("Kwadrat: ", n * n);
```

## `zegar`

Zwraca obecny czas UTC w sekundach jako liczbę. Przydaje się do mierzenia czasu wykonania:

```text
zmienna start = zegar();
// ... wykonywanie operacji ...
wypisz("Czas: ", zegar() - start);
```

## `liczba`

Zamienia wartość na liczbę. Błędny argument (np. `liczba("abc")`) kończy się błędem.

## `napis`

Zamienia wartość na napis (`napis(42)` daje `"42"`).

## `typ`

Zwraca napis opisujący typ wartości, np. `LICZBA` dla `typ(5)`.