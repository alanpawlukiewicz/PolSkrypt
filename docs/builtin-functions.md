# Funkcje wbudowane PolSkrypt

Wersja dokumentu: 1.0.0

Część referencji języka PolSkrypt (zob. [language-reference.md](language-reference.md)).

Funkcje wbudowane są dostępne globalnie, bez żadnych deklaracji, także w każdym importowanym module. Metody natywne list (`dodaj`, `długość` itd.) są opisane osobno w [lists.md](lists.md), a klasa `Plik` w [files.md](files.md).

Funkcje wbudowane znajdują się w osobnym zasięgu, więc zmienna o tej samej nazwie (np. `zmienna typ = 1;`) przesłania je w danym pliku, zamiast powodować błąd.

## Zestawienie

| Funkcja | Opis | Przykład | Wynik |
|---------|------|----------|-------|
| `wypisz(x, ...)` | wypisuje wartości oddzielone spacją; **nie** dodaje na końcu znaku nowej linii; obsługuje sekwencje specjalne `\n`, `\t`, `\r` | `wypisz(1, "a");` | `1 a` |
| `wprowadź()` | pozwala na wprowadzenie wartości zmiennej z klawiatury w postaci napisu | `zmienna a = wprowadź();` | |
| `zegar()` | zwraca obecną godzinę UTC w sekundach w postaci liczby | `wypisz(zegar());` | `1793629096` |
| `liczba(x)` | zamienia wartość `x` na liczbę; zwraca błąd w przypadku niepoprawnego argumentu | `liczba("42");` | `42` |
| `napis(x)` | zamienia wartość `x` na napis | `napis(42);` | `"42"` |
| `typ(x)` | zwraca typ wartości `x` | `wypisz(typ(5));` | `LICZBA` |
| `jest_instancją(x, K)` | sprawdza, czy `x` jest instancją klasy `K` lub klasy po niej dziedziczącej | `jest_instancją(Pies(), Zwierze);` | `PRAWDA` |
| `klasa_obiektu(x)` | zwraca klasę, której instancją jest `x`; dla innych wartości `wartość_pusta` | `klasa_obiektu(Pies());` | `<klasa Pies>` |

## `wypisz`

Wypisuje podane wartości na standardowe wyjście. Wartości są oddzielone pojedynczą spacją, a po ostatniej wartości nie jest dodawany żaden znak (do wersji 0.4.1 wywołanie z wieloma argumentami zostawiało na końcu zbędną spację). Wywołanie bez argumentów nic nie wypisuje.

Sekwencje `\n` (nowa linia), `\t` (tabulator) i `\r` (powrót karetki) zapisane w napisie są zamieniane na odpowiadające im znaki. Robi to tylko `wypisz`: inne operacje, np. zapis do pliku metodą `wpisz`, zachowują je dosłownie.

`wypisz` nie kończy wypisanego tekstu znakiem nowej linii. Żeby przejść do nowej linii, użyj sekwencji `\n`:

```text
wypisz("a");            // wypisze: a
wypisz("b");            // wypisze: b (w tej samej linii, razem: ab)
wypisz("c\n");          // wypisze: c i przejdzie do nowej linii
wypisz(1, "a", prawda); // wypisze: 1 a PRAWDA
wypisz([1, 2, 3]);      // wypisze: [1, 2, 3]
wypisz("a\tb");         // wypisze: a, tabulator, b
```

Wartości są zamieniane na tekst tak samo jak przez funkcję [`napis`](#napis): liczby całkowite bez części dziesiętnej (`wypisz(3.0)` wypisze `3`), ułamki zawsze z kropką (`wypisz(2.5)` wypisze `2.5`), a wartości logiczne jako `PRAWDA` i `FAŁSZ`.

## `wprowadź`

Pobiera wartość wprowadzoną z klawiatury i zwraca ją jako **napis**. Aby użyć jej jako liczby, przekonwertuj ją funkcją `liczba`:

```text
wypisz("Podaj liczbę: ");
zmienna n = liczba(wprowadź());
wypisz("Kwadrat: ", n * n);
```

Po osiągnięciu końca wejścia (np. gdy na wejście programu przekierowano plik) `wprowadź` zwraca `wartość_pusta`.

## `zegar`

Zwraca obecny czas UTC w sekundach jako liczbę. Przydaje się do mierzenia czasu wykonania:

```text
zmienna start = zegar();
// ... wykonywanie operacji ...
wypisz("Czas: ", zegar() - start);
```

## `liczba`

Zamienia wartość na liczbę:

- liczba jest zwracana bez zmian;
- napis jest odczytywany jako liczba **z kropką dziesiętną**, niezależnie od ustawień językowych systemu. Dozwolone są znak `+` lub `-`, zapis wykładniczy i białe znaki na początku i końcu.

| Wywołanie | Wynik |
|-----------|-------|
| `liczba(5)` | `5` |
| `liczba("42")` | `42` |
| `liczba("3.5")` | `3.5` |
| `liczba("  -7 ")` | `-7` |
| `liczba("1e3")` | `1000` |
| `liczba("3,5")` | *błąd* (przecinek nie jest separatorem dziesiętnym ani tysięcy) |
| `liczba("abc")` | *błąd* |
| `liczba(prawda)`, `liczba(wartość_pusta)`, `liczba([1])` | *błąd* |

Błąd ma komunikat `Argument musi być liczbą.` i wskazuje linię wywołania.

Dla każdej liczby `x` zachodzi `liczba(napis(x)) == x`.

## `napis`

Zamienia wartość na napis.

| Wartość | Wynik |
|---------|-------|
| "abc" | "abc" |
| wartość_pusta | "WARTOŚĆ PUSTA" |
| 42 | "42" |
| 3.5 | "3.5" (zawsze z kropką) |
| 0.1 + 0.2 | "0.30000000000000004" |
| 10 ** 400 | "nieskończoność" |
| -(10 ** 400) | "-nieskończoność" |
| wynik nieokreślony (np. `10 ** 400 - 10 ** 400`) | "nie-liczba" |
| prawda | "PRAWDA" |
| fałsz | "FAŁSZ" |
| [1, 2, 3] | "[1, 2, 3]" |
| funkcja `f` | "\<funkcja f\>" |
| funkcja anonimowa | "\<funkcja anonimowa\>" |
| funkcja wbudowana | "\<funkcja natywna\>" |
| metoda listy, np. `l.dodaj` | "\<funkcja listy dodaj\>" |
| metoda pliku, np. `f.czytaj` | "\<funkcja pliku czytaj\>" |
| klasa | "\<klasa K\>" |
| klasa `Plik` | "\<klasa natywna Plik\>" |
| instancja klasy | "\<instancja klasy K\>" |
| moduł | "\<moduł nazwa\>" |
| otwarty plik | "PLIK" |

Liczby są zapisywane w najkrótszej postaci, która przy ponownym odczycie przez `liczba` daje dokładnie tę samą wartość.

## `typ`

Zwraca napis opisujący rodzaj wartości. Wynik jest zawsze jedną z poniższych stałych i nie zależy od nazw funkcji ani klas, więc `typ(f) == typ(g)` jest prawdą dla dowolnych dwóch funkcji.

| Wartość | Wynik |
|---------|-------|
| liczba | LICZBA |
| napis | NAPIS |
| prawda, fałsz | WARTOŚĆ LOGICZNA |
| wartość_pusta | WARTOŚĆ PUSTA |
| lista | LISTA |
| funkcja (nazwana, anonimowa, natywna, metoda, metoda listy) | FUNKCJA |
| klasa (także `Plik`) | KLASA |
| instancja dowolnej klasy | INSTANCJA |
| moduł | MODUŁ |
| otwarty plik | PLIK |

Żeby sprawdzić, jakiej klasy jest obiekt, użyj `jest_instancją` lub `klasa_obiektu`.

## `jest_instancją`

Sprawdza, czy wartość jest instancją podanej klasy **lub klasy, która po niej dziedziczy**. Dla wartości, które nie są instancjami, zwraca `fałsz`. Drugi argument musi być klasą (także `Plik`: otwarty plik jest instancją klasy `Plik`).

```text
klasa Zwierze {}
klasa Pies dziedziczy_po Zwierze {}

jest_instancją(Pies(), Pies);      // prawda
jest_instancją(Pies(), Zwierze);   // prawda
jest_instancją(Zwierze(), Pies);   // fałsz
jest_instancją(5, Pies);           // fałsz
```

Klasy są porównywane jako obiekty, a nie po nazwie: dwie różne klasy o tej samej nazwie (np. z dwóch modułów) są różne.

## `klasa_obiektu`

Zwraca **klasę**, której instancją jest wartość, czyli dokładną klasę, bez przechodzenia do klas nadrzędnych. Dla otwartego pliku zwraca klasę `Plik`. Dla wartości, które nie są instancjami, zwraca `wartość_pusta`.

Wynik jest zwykłą klasą: można go porównać, wywołać, żeby utworzyć nowy obiekt tej samej klasy, albo użyć do wywołania metody statycznej.

```text
klasa Zwierze { statyczna metoda gatunek() { zwróć "zwierzę"; } }
klasa Pies dziedziczy_po Zwierze { konstruktor(imie) { ja.imie = imie; } }

zmienna burek = Pies("Burek");
klasa_obiektu(burek) == Pies;      // prawda
klasa_obiektu(burek) == Zwierze;   // fałsz (w przeciwieństwie do jest_instancją)
zmienna azor = klasa_obiektu(burek)("Azor");  // nowy Pies
klasa_obiektu(burek).gatunek();    // "zwierzę"
wypisz(klasa_obiektu(burek));      // <klasa Pies>
klasa_obiektu(5);                  // wartość_pusta
```