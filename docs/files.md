# Pliki w PolSkrypt

Wersja dokumentu: 1.0.0

Część referencji języka PolSkrypt (zob. [language-reference.md](language-reference.md)).

## Spis treści

1. [Klasa `Plik`](#1-klasa-plik)
2. [Otwieranie pliku](#2-otwieranie-pliku)
3. [Metody otwartego pliku](#3-metody-otwartego-pliku)
4. [Kodowanie i pozycja w pliku](#4-kodowanie-i-pozycja-w-pliku)
5. [Przykłady](#5-przykłady)
6. [Błędy](#6-błędy)

---

## 1. Klasa `Plik`

Operacje na plikach udostępnia wbudowana klasa `Plik`. Wszystkie jej metody są statyczne, więc wywołuje się je bezpośrednio na klasie:

| Metoda | Opis | Wynik |
|--------|------|-------|
| `Plik.otwórz(ścieżka, tryb)` | otwiera plik w podanym trybie (zob. niżej) | otwarty plik |
| `Plik.istnieje(ścieżka)` | sprawdza, czy plik istnieje (dla katalogu zwraca `fałsz`) | `prawda` / `fałsz` |
| `Plik.usuń(ścieżka)` | usuwa plik; usunięcie nieistniejącego pliku nie jest błędem | `wartość_pusta` |
| `Plik.przemianuj(stara, nowa)` | zmienia nazwę pliku lub przenosi go; błąd, jeśli plik docelowy już istnieje | `wartość_pusta` |

Ścieżki względne są liczone od katalogu, z którego uruchomiono interpreter.

```text
jeżeli (Plik.istnieje("dane.txt")) {
    Plik.przemianuj("dane.txt", "dane_stare.txt");
}
```

---

## 2. Otwieranie pliku

```text
zmienna f = Plik.otwórz("dane.txt", "r");
```

| Tryb | Odczyt | Zapis | Gdy plik nie istnieje | Gdy plik istnieje |
|------|--------|-------|------------------------|-------------------|
| `"r"` | tak | nie | błąd | otwiera od początku |
| `"w"` | nie | tak | tworzy nowy | **czyści zawartość** |
| `"a"` | nie | tak | tworzy nowy | dopisuje na końcu |
| `"rw"` | tak | tak | tworzy nowy | otwiera od początku bez czyszczenia |

Otwarty plik jest wartością typu `PLIK` (`typ(f)` zwraca `PLIK`). Jest instancją klasy `Plik`: `jest_instancją(f, Plik)` zwraca `prawda`, a `klasa_obiektu(f)` zwraca `Plik`.

Po zakończeniu pracy plik należy zamknąć metodą `zamknij()`. Każda operacja na zamkniętym pliku kończy się błędem.

---

## 3. Metody otwartego pliku

### Odczyt

| Metoda | Opis |
|--------|------|
| `czytaj()` | zwraca całą pozostałą zawartość pliku (od bieżącej pozycji do końca) |
| `czytaj_linie()` | zwraca następną linię bez znaku końca linii (`\n` lub `\r\n`); na końcu pliku zwraca `wartość_pusta` |
| `czytaj_znaki(n)` | zwraca następne `n` znaków (mniej, jeśli plik się skończy) |
| `koniec()` | zwraca `prawda`, jeśli bieżąca pozycja jest na końcu pliku |

### Zapis

| Metoda | Opis |
|--------|------|
| `wpisz(x)` | zapisuje wartość `x` zamienioną na napis (jak `napis(x)`) |
| `wpisz_linie(x)` | zapisuje wartość `x` i znak nowej linii |
| `przytnij()` | usuwa całą zawartość pliku od bieżącej pozycji do końca |
| `opróżnij()` | wymusza zapis buforowanych danych na dysk (zapis i tak następuje od razu po każdym `wpisz`) |

Metody zapisu i metody zmieniające pozycję zwracają ten sam plik, więc można je łączyć:

```text
zmienna f = Plik.otwórz("wynik.txt", "w");
f.wpisz("Wynik: ").wpisz(42).wpisz_linie("").wpisz_linie("koniec");
f.zamknij();
```

> **Uwaga:** sekwencje `\n`, `\t`, `\r` są zamieniane na znaki specjalne tylko przez `wypisz`. `wpisz("a\nb")` zapisze do pliku dosłownie cztery znaki `a`, `\`, `n`, `b`. Nową linię w pliku zapisuje się metodą `wpisz_linie`.

### Pozycja i stan

| Metoda | Opis |
|--------|------|
| `pozycja()` | zwraca bieżącą pozycję w bajtach od początku pliku |
| `ustaw(n)` | ustawia pozycję na `n` bajtów od początku pliku |
| `przewiń()` | wraca na początek pliku (to samo, co `ustaw(0)`) |
| `rozmiar()` | zwraca rozmiar pliku w bajtach |
| `zamknij()` | zamyka plik; ponowne zamknięcie nie jest błędem |
| `zamknięty()` | zwraca `prawda`, jeśli plik został zamknięty |

Metody odczytu wymagają trybu `"r"` lub `"rw"`, a metody zapisu (`wpisz`, `wpisz_linie`, `przytnij`) trybu `"w"`, `"a"` lub `"rw"`.

---

## 4. Kodowanie i pozycja w pliku

- Pliki są zapisywane i odczytywane w kodowaniu **UTF-8** (bez znacznika BOM). Znacznik BOM na początku czytanego pliku jest pomijany.
- `czytaj_znaki(n)` liczy **znaki**, więc poprawnie czyta polskie litery, które w UTF-8 zajmują po 2 bajty.
- `pozycja()`, `ustaw(n)` i `rozmiar()` operują na **bajtach**. Napis `"żółw"` ma 4 znaki, ale zajmuje w pliku 7 bajtów.
- W trybie `"a"` nie można ustawić pozycji przed końcem pliku.

---

## 5. Przykłady

### Odczyt pliku linia po linii

```text
zmienna f = Plik.otwórz("dane.txt", "r");
zmienna numer = 1;
zmienna linia = f.czytaj_linie();

podczas_gdy (linia != wartość_pusta) {
    wypisz(numer, linia, "\n");
    numer += 1;
    linia = f.czytaj_linie();
}

f.zamknij();
```

### Zapis listy do pliku

```text
zmienna liczby = [3, 1, 4, 1, 5];
zmienna f = Plik.otwórz("liczby.txt", "w");

dla (zmienna i = 0; i < liczby.długość(); i += 1) {
    f.wpisz_linie(liczby[i]);
}

f.zamknij();
```

### Odczyt i modyfikacja w trybie `"rw"`

```text
zmienna f = Plik.otwórz("notatki.txt", "rw");
zmienna tresc = f.czytaj();     // pozycja jest teraz na końcu pliku
f.wpisz_linie("nowa linia");    // dopisanie na końcu
f.ustaw(5);
f.przytnij();                   // zostawia tylko pierwsze 5 bajtów
f.przewiń();
wypisz(f.czytaj());             // odczyt od początku
f.zamknij();
```

---

## 6. Błędy

Wszystkie błędy związane z plikami są błędami czasu działania i wskazują linię wywołania.

| Sytuacja | Komunikat |
|----------|-----------|
| nieznany tryb otwarcia | `Nieprawidłowy tryb pliku. Dozwolone: r, w, a, rw.` |
| pusta nazwa pliku | `Nazwa pliku nie może być pusta.` |
| ścieżka wskazuje na katalog | `Podana ścieżka wskazuje na katalog, a nie na plik.` |
| plik do odczytu nie istnieje (`"r"`) | `Plik nie istnieje.` |
| nieistniejący katalog w ścieżce | `Nieprawidłowa ścieżka pliku.` |
| brak uprawnień | `Prośba o dostęp do pliku została odrzucona.` |
| plik używany przez inny proces | `Nie można otworzyć pliku (może być używany przez inny proces).` |
| operacja na zamkniętym pliku | `Plik jest zamknięty.` |
| odczyt z pliku otwartego tylko do zapisu | `Plik nie jest otwarty w trybie czytania.` |
| zapis do pliku otwartego tylko do odczytu | `Plik nie jest otwarty w trybie zapisu.` |
| ujemna lub ułamkowa pozycja w `ustaw` | `Indeks musi być nieujemną liczbą całkowitą.` |
| ujemna lub ułamkowa liczba znaków w `czytaj_znaki` | `Liczba znaków musi być nieujemną liczbą całkowitą.` |
| nieznana metoda pliku | `Podano nieprawidłową funkcję pliku.` |
| `przemianuj`, gdy plik docelowy istnieje | `Nie można przemianować pliku (docelowy już istnieje lub plik jest używany).` |
