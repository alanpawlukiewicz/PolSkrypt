# Listy w PolSkrypt

Wersja dokumentu: 0.4.0

Część referencji języka PolSkrypt (zob. [language-reference.md](language-reference.md)).

## Spis treści

1. [Tworzenie list](#1-tworzenie-list)
2. [Indeksowanie](#2-indeksowanie)
3. [Semantyka referencyjna](#3-semantyka-referencyjna)
4. [Metody natywne list](#4-metody-natywne-list)
5. [Błędy](#5-błędy)

---

## 1. Tworzenie list

Listę tworzy się literałem w nawiasach kwadratowych. Elementy są oddzielone przecinkami i mogą być dowolnymi wyrażeniami, a także wartościami różnych typów.

```text
zmienna pusta = [];
zmienna liczby = [1, 2, 3];
zmienna mieszana = [1 + 1, "a" & 1, prawda, wartość_pusta];
zmienna zagniezdzona = [[1, 2], [3, 4], []];
```

Zasady:

- Dozwolony jest przecinek po ostatnim elemencie: `[1, 2,]` to to samo, co `[1, 2]`.
- Każde obliczenie literału tworzy **nową** listę. Funkcja zwracająca `[1]` zwraca przy każdym wywołaniu inną listę.
- Elementami mogą być też funkcje, klasy i instancje (np. `[funkcjaA, funkcjaB]`, `[Punkt(1, 2)]`).
- Lista, także pusta, jest wartością prawdziwą (zob. [Prawdziwość wartości](language-reference.md#prawdziwość-wartości)). Do sprawdzenia, czy lista jest pusta, służy metoda `pusta()`.

---

## 2. Indeksowanie

Elementy numeruje się od zera. Indeks podaje się w nawiasach kwadratowych po nazwie zmiennej. Może to być dowolne wyrażenie o wartości liczbowej.

```text
zmienna l = [10, 20, 30];

wypisz(l);              // wypisze: [10, 20, 30]
wypisz(l[0]);           // wypisze: 10
wypisz(l[1 + 1]);       // wypisze: 30

zmienna i = 1;
wypisz(l[i]);           // wypisze: 20
```

### Zapis elementu

Przypisanie do `lista[i]` zmienia element listy. Przypisanie jest wyrażeniem i zwraca przypisaną wartość, więc można je łączyć:

```text
zmienna l = [1, 2, 3];
l[0] = 10;              // l == [10, 2, 3]
l[1] = l[2] = 7;        // l == [10, 7, 7]
```

### Listy zagnieżdżone

```text
zmienna m = [[1, 2], [3, 4]];
zmienna wiersz = m[1];  // wiersz == [3, 4]
wypisz(wiersz[0]);      // wypisze: 3
```

### Ograniczenie: indeksowanie tylko po nazwie zmiennej

Indeksowanie działa obecnie wyłącznie bezpośrednio po nazwie zmiennej. Poniższe zapisy zgłaszają błąd składni (`Oczekiwano ';' po wartości.`):

```text
m[0][1];                // błąd: indeksowanie wyniku indeksowania
obiekt.pole[0];         // błąd: indeksowanie pola
funkcja()[0];           // błąd: indeksowanie wyniku wywołania
[1, 2, 3][0];           // błąd: indeksowanie literału
```

Obejściem jest zapisanie wyniku w zmiennej pomocniczej:

```text
zmienna wiersz = m[0];
wypisz(wiersz[1]);
```

Po zaindeksowaniu można za to użyć dostępu do pola lub wywołania: `lista[0].pole`, `lista[0].metoda()`, `lista[0](argument)` (gdy element jest funkcją).

---

## 3. Semantyka referencyjna

Zmienna przechowuje referencję do listy, a nie jej kopię. Przypisanie, przekazanie do funkcji i zwrócenie z funkcji nie kopiują listy:

```text
zmienna a = [1, 2];
zmienna b = a;
b[0] = 9;
wypisz(a[0]);           // wypisze: 9 (a i b to ta sama lista)

funkcja ustaw(l) {
    l[0] = 5;
}
ustaw(a);
wypisz(a[0]);           // wypisze: 5 (funkcja zmieniła oryginalną listę)
```

Porównanie `==` sprawdza, czy obie strony to **ta sama** lista, a nie czy mają równą zawartość:

```text
zmienna a = [1];
zmienna b = a;
wypisz(a == b);         // wypisze: prawda
wypisz(a == [1]);       // wypisze: fałsz
```

Aby uzyskać niezależną kopię, użyj metody `kopiuj()` (zob. niżej).

---

## 4. Metody natywne list

Na listach można wywoływać metody natywne za pomocą operatora `.`:

```text
zmienna l = [1, 2];
l.dodaj(3);
```

| Metoda | Opis |
|--------|------|
| `dodaj(...)` | dodaje elementy na koniec listy, zwraca zmodyfikowaną listę |
| `wstaw(i, x)` | wstawia element `x` na pozycję `i`, zwraca zmodyfikowaną listę |
| `usuń(i)` | usuwa element z listy po indeksie, zwraca zmodyfikowaną listę |
| `długość()` | zwraca liczbę elementów listy |
| `pusta()` | zwraca `prawda`, jeśli lista nie ma elementów |
| `zawiera(x)` | zwraca `prawda`, jeśli lista zawiera wartość `x` |
| `szukaj(x)` | zwraca indeks pierwszego wystąpienia `x` |
| `policz(x)` | zwraca liczbę wystąpień wartości `x` w liście |
| `kopiuj()` | zwraca nową listę z tymi samymi elementami (kopia płytka) |
| `wyczyść()` | usuwa wszystkie elementy listy, zwraca zmodyfikowaną listę |

### Przykład

```text
zmienna l = [10, 20, 30];
zmienna suma = 0;

dla (zmienna i = 0; i < l.długość(); i += 1) {
    suma += l[i];
}

wypisz(suma);           // wypisze: 60
```

---

## 5. Błędy

Błędy czasu działania związane z listami:

| Sytuacja | Przykład |
|----------|----------|
| odczyt lub zapis poza zakresem listy | `zmienna l = [1, 2, 3]; l[3];`, `l[3] = 1;`, `zmienna p = []; p[0];` |
| indeksowanie wartości, która nie jest listą | `zmienna x = 5; x[0];`, `x[0] = 1;`, także dla napisów i `wartość_pusta` |
| zapis atrybutu na liście | `l.pole = 1;` |
| wpisanie indeksu jako liczba ujemna lub zmiennoprzecinkowa | `l[-1]`, `l[1.5]` |

Błędy wykrywane przed uruchomieniem (resolver, zob. [tools.md](tools.md)):

- odczyt listy w jej własnym inicjatorze: `{ zmienna b = b[0]; }`.