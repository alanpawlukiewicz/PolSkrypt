# Klasy i dziedziczenie w PolSkrypt

Wersja dokumentu: 0.4.0

Część referencji języka PolSkrypt (zob. [language-reference.md](language-reference.md)).

## Spis treści

1. [Klasy](#1-klasy)
2. [Dziedziczenie](#2-dziedziczenie)
3. [Błędy związane z klasami](#3-błędy-związane-z-klasami)

---

## 1. Klasy

### Składnia

```text
klasa Nazwa {
    konstruktor(parametry) { ciało }

    metoda nazwa(parametry) { ciało }

    statyczna metoda nazwa(parametry) { ciało }
}
```

W ciele klasy mogą występować wyłącznie: konstruktor, metody i metody statyczne. Po definicji klasy ani jej elementów nie stawia się średnika. Klasę można zdefiniować także lokalnie (w bloku lub w funkcji); wtedy jej metody widzą zmienne z otaczającego zasięgu.

### Elementy klasy

| Element | Składnia | Opis |
|---------|----------|------|
| Konstruktor | `konstruktor(a, b) { ... }` | wywoływany przy tworzeniu instancji; **nie** poprzedza go słowo `metoda` |
| Metoda | `metoda nazwa(a) { ... }` | metoda instancji; każda zwykła metoda **musi** zaczynać się od słowa `metoda` |
| Metoda statyczna | `statyczna metoda nazwa(a) { ... }` | metoda wywoływana na samej klasie, a nie na instancji; poprzedzona połączonymi słowami `statyczna metoda` |

### Tworzenie instancji

Klasę wywołuje się jak funkcję. Wywołanie tworzy nową instancję i wykonuje jej konstruktor z podanymi argumentami. Liczba argumentów musi zgadzać się z liczbą parametrów konstruktora.

```text
zmienna a = K(1, 2);
```

Klasa jest wartością, więc można ją przypisać do zmiennej i tworzyć instancje przez tę zmienną: `zmienna Inna = K; zmienna b = Inna(1, 2);`.

### Pola i słowo `ja`

Wewnątrz konstruktora i metod słowo kluczowe `ja` oznacza bieżącą instancję. Pola tworzy się przez przypisanie i odczytuje przez operator `.`:

```text
ja.a = a;               // zapis pola (wewnątrz konstruktora lub metody)
zmienna x = obiekt.a;   // odczyt pola
```

PolSkrypt wspiera dynamiczne dodawanie atrybutów:

```text
klasa K {}
zmienna a = K();
a.b = 3;
wypisz(a.b); // wypisze: 3
```

Próba odwołania się do nieistniejącego atrybutu lub metody skończy się błędem:

```text
klasa K {}
zmienna a = K();
wypisz(a.b);     // błąd
wypisz(a.c());   // błąd
```

### Wywoływanie metod

- metody instancji wywołuje się na instancji: `a.metoda1();`
- metody statyczne wywołuje się na klasie: `K.st_metoda1();`
- metoda pobrana jako wartość (`zmienna m = a.metoda1;`) pozostaje związana z instancją, więc `m()` działa na tej samej instancji `a`.
- metoda może zwrócić `ja`, co pozwala łączyć wywołania w łańcuch: `licznik.zwieksz().zwieksz();`

Dostęp przez `.` jest możliwy także na elemencie listy: `lista[0].metoda()` (zob. [lists.md](lists.md)).

### Przykład

```text
klasa K {
    metoda metoda1() {
        wypisz("zwykła metoda");
    }

    statyczna metoda st_metoda1() {
        wypisz("statyczna metoda");
    }

    konstruktor(a, b) {
        ja.a = a;
        ja.b = b;
    }
}

zmienna a = K(1, 2);
wypisz(a.a);          // wypisze: 1
wypisz(a.b);          // wypisze: 2
a.metoda1();          // wypisze: zwykła metoda
K.st_metoda1();       // wypisze: statyczna metoda
```

### Zasady

- Metoda bez `zwróć` zwraca `wartość_pusta`.
- Konstruktor **zawsze** zwraca instancję obiektu którego tworzy. Wewnątrz konstruktora dozwolone jest samo `zwróć;`, ale `zwróć wartość;` jest błędem.
- `ja` jest dostępne wyłącznie wewnątrz klasy i tylko w konstruktorze oraz metodach instancji. W metodach statycznych (także w funkcjach zagnieżdżonych w nich) `ja` jest błędem, bo nie ma wtedy instancji.
- W jednej klasie nie można zdefiniować dwóch metod o tej samej nazwie. Dotyczy to osobno metod zwykłych i osobno metod statycznych; metoda zwykła i statyczna mogą mieć tę samą nazwę.
- Klasa i instancja są wartościami prawdziwymi (zob. [Prawdziwość wartości](language-reference.md#prawdziwość-wartości)).
- Instancje są porównywane przez referencję.

---

## 2. Dziedziczenie

Klasa może dziedziczyć po innej klasie. Służy do tego słowo kluczowe `dziedziczy_po`, umieszczone po nazwie klasy:

```text
klasa Pochodna dziedziczy_po Bazowa {
    ...
}
```

Po `dziedziczy_po` może wystąpić tylko jedna nazwa klasy (sama nazwa, a nie wyrażenie). Klasa bazowa musi być zdefiniowana wcześniej niż klasa pochodna.

### Co jest dziedziczone

- Klasa pochodna ma wszystkie metody klasy bazowej, także tych odziedziczonych dalej w łańcuchu hierarchii (`C` dziedziczy po `B`, a `B` po `A`, więc `C` ma metody `A`).
- Metodę można **nadpisać**, definiując w klasie pochodnej metodę o tej samej nazwie. Wersja z klasy pochodnej ma pierwszeństwo.
- Pola nie są deklarowane w klasie, tylko tworzone przez przypisanie (zob. wyżej), więc istnieją w instancji niezależnie od tego, w której klasie hierarchii zostały przypisane.
- Klasa pochodna korzysta z konstruktora klasy bazowej oraz może go **nadpisać**.

```text
klasa A {
    metoda f() { zwróć "A"; }
}

klasa B dziedziczy_po A { }

wypisz(B().f());      // wypisze: A
```

### Polimorfizm

`ja` w odziedziczonej metodzie zawsze wskazuje instancję rzeczywistej klasy, więc wywołanie `ja.metoda()` uwzględnia nadpisania:

```text
klasa A {
    metoda opisz() { zwróć "Jestem " & ja.nazwa(); }
    metoda nazwa() { zwróć "A"; }
}

klasa B dziedziczy_po A {
    metoda nazwa() { zwróć "B"; }
}

wypisz(B().opisz());  // wypisze: Jestem B
```

### Słowo kluczowe `super`

`super.nazwa(...)` wywołuje metodę z klasy bazowej, nawet jeśli klasa pochodna ją nadpisała. Wywołanie działa na bieżącej instancji (`ja`).

```text
klasa A {
    metoda f() { zwróć "A"; }
}

klasa B dziedziczy_po A {
    metoda f() { zwróć super.f() & "B"; }
}

klasa C dziedziczy_po B {
    metoda f() { zwróć super.f() & "C"; }
}

wypisz(C().f());      // wypisze: ABC
```

`super` odnosi się do klasy bazowej **tej klasy, w której zostało zapisane**, a nie klasy instancji. Dzięki temu `C().f()` wywołuje `B.f`, a ta wywołuje `A.f`, zamiast zapętlać się w `B.f`.

`super` można używać także wewnątrz funkcji zagnieżdżonych i anonimowych zdefiniowanych w metodzie.

#### Wywołanie konstruktora klasy bazowej

Konstruktor to zwykła metoda o nazwie `konstruktor`, więc można go wywołać przez `super.konstruktor(...)`. Typowo robi się to na początku konstruktora klasy pochodnej:

```text
klasa Zwierze {
    konstruktor(imie) {
        ja.imie = imie;
    }

    metoda powiedz() {
        zwróć ja.imie & " robi dźwięk";
    }
}

klasa Pies dziedziczy_po Zwierze {
    konstruktor(imie, rasa) {
        super.konstruktor(imie);
        ja.rasa = rasa;
    }

    metoda powiedz() {
        zwróć super.powiedz() & " hau";
    }
}

zmienna p = Pies("Burek", "kundel");
wypisz(p.powiedz());  // wypisze: Burek robi dźwięk hau
wypisz(p.rasa);       // wypisze: kundel
```

Konstruktor klasy bazowej nie jest wywoływany automatycznie (wymaga użycia `super.konstruktor(...)`).

#### Ograniczenia `super`

- `super` służy do dostępu do **metod**, nie do pól. `super.pole` jest błędem.
- `super` jest dozwolone tylko w klasie, która ma klasę bazową (`dziedziczy_po`). Użycie poza klasą lub w klasie bez klasy bazowej jest błędem.
- `super` nie można używać w metodach statycznych.
- Jeśli klasa bazowa nie ma wskazanej metody, `super.metoda()` kończy się błędem w czasie działania programu.

---

## 3. Błędy związane z klasami

### Błędy wykrywane przed uruchomieniem (resolver)

Poniższe błędy są zgłaszane przez analizę statyczną (zob. [tools.md](tools.md)):

| Sytuacja | Przykład |
|----------|----------|
| `ja` poza klasą | `funkcja f() { zwróć ja; }` |
| `ja` w metodzie statycznej | `statyczna metoda f() { zwróć ja; }` |
| `super` poza klasą | `super.f();` |
| `super` w klasie bez klasy bazowej | `klasa A { metoda f() { super.f(); } }` |
| `super` w metodzie statycznej | `statyczna metoda f() { super.g(); }` |
| klasa dziedziczy po samej sobie | `klasa A dziedziczy_po A { }` |
| zwracanie wartości z konstruktora | `konstruktor() { zwróć 1; }` |
| dwie metody o tej samej nazwie w klasie | `metoda f() { } metoda f() { }` |
| dwie metody statyczne o tej samej nazwie | `statyczna metoda f() { } statyczna metoda f() { }` |

### Błędy czasu działania

- Odczyt nieistniejącego atrybutu lub metody instancji.
- Wywołanie metody statycznej, której klasa nie ma (także gdy istnieje metoda zwykła o tej nazwie).
- Odczyt właściwości (`x.y`) z wartości, która nie jest instancją, klasą ani listą (np. liczby lub napisu).
- Zapis atrybutu (`x.y = 1`) na wartości, która nie jest instancją (np. na klasie, liście lub liczbie).
- Wywołanie konstruktora z niepoprawną liczbą argumentów.
- Klasa bazowa, która nie jest klasą (np. `zmienna X = 5; klasa A dziedziczy_po X { }`) albo nie jest zdefiniowana.
- `super.metoda()`, gdy klasa bazowa nie ma takiej metody.