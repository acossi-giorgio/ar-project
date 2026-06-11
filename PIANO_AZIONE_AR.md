# Piano di azione progetto Augmented Reality

## Contesto e vincoli del corso

Questo piano e' basato sui PDF presenti nella cartella `slides`, in particolare `slides/rules.pdf`. Nel workspace non e' presente `notes.pdf`; se verra' aggiunto, il piano andra' ricontrollato rispetto a eventuali requisiti extra.

Requisiti individuati:

- Creare un'applicazione per un ambiente aumentato: room, office, park o street.
- Rilevare immagini, feature, oggetti o piani e aumentarli con informazioni interessanti, divertenti o utili.
- Usare smartphone/tablet e test in ambienti reali, oppure XR simulation/digital twin. Per questo progetto si sceglie test reale su smartphone.
- Testare l'app con un numero ragionevole di feature visive.
- Registrare un video dell'app in esecuzione.
- Consegnare un report finale con scelte di librerie, SDK, design e implementazione.
- Consegnare link a repository con progetto pulito.
- Consegnare almeno 5 giorni lavorativi prima dell'esame.

## Idea di progetto proposta

Titolo provvisorio: **Augmented Study Room**

L'app trasforma una stanza di studio o un piccolo ufficio in un ambiente aumentato. Lo smartphone riconosce piani reali e target visivi presenti nella stanza, poi sovrappone contenuti utili e interattivi.

Esempi di contenuti:

- Su una scrivania rilevata come piano orizzontale: pannello 3D con agenda, timer di studio o checklist.
- Su immagini target stampate, poster, copertine di libri o appunti: card AR con spiegazioni, link testuali, modelli 3D o animazioni.
- Su una parete rilevata come piano verticale: bacheca virtuale con reminder, note o etichette.
- Su oggetti o zone scelte della stanza: marker virtuali persistenti durante la sessione.

Questa idea segue bene la traccia "Augmented Room/Office" e permette di mostrare piu' tecniche AR senza dipendere da un digital twin.

## Scelte tecnologiche

Scelta principale: **Unity + AR Foundation + ARKit XR Plugin + ARCore XR Plugin**.

Motivazione:

- Le slide del corso introducono AR Foundation come SDK principale per ARCore e ARKit.
- AR Foundation permette di sviluppare una sola codebase Unity e usare ARKit su iOS e ARCore su Android.
- ARKit e ARCore supportano le feature principali richieste: world tracking, plane detection, image tracking, raycast e, dove disponibile, light estimation.
- Il progetto resta coerente con la scelta dello smartphone come main device e documenta le differenze tra le due piattaforme.

Target:

- Dispositivi: almeno un iPhone compatibile ARKit e almeno uno smartphone Android compatibile ARCore.
- Piattaforme: iOS e Android.
- Ambiente di test: stanza reale o ufficio reale.
- Nessun digital twin e nessun test principale in simulazione.
- XR Simulation opzionale solo per debug iniziale, non come validazione finale.

## Feature AR minime da implementare

Per soddisfare il requisito di un numero ragionevole di feature visive, implementare almeno:

1. **Plane detection**
   - Rilevamento di piani orizzontali per scrivania/pavimento.
   - Rilevamento di piani verticali per pareti, se supportato stabilmente dal dispositivo.
   - Visualizzazione discreta del piano rilevato durante la fase di setup.

2. **Raycast e placement**
   - Tap sul piano reale per posizionare un contenuto 3D.
   - Possibilita' di spostare o sostituire il contenuto.

3. **Image tracking**
   - Libreria con almeno 4-6 immagini target stampate o presenti nell'ambiente reale.
   - Ogni immagine deve mostrare un contenuto AR diverso.
   - Le immagini target devono avere buon contrasto, dettagli ricchi e pochi pattern ripetitivi.

4. **Contenuti aumentati**
   - Almeno 3 tipi di augmentations:
     - testo o pannello informativo;
     - modello 3D o prefab animato;
     - elemento interattivo, per esempio bottone, toggle, checklist o timer.

5. **Stabilita' e usabilita'**
   - I contenuti devono rimanere ancorati in modo credibile al mondo reale.
   - UI semplice per avviare scansione, mostrare stato tracking e resettare la scena.

Feature opzionali, se rimane tempo:

- Light estimation per integrare meglio gli oggetti 3D.
- Occlusion, se il dispositivo supporta LiDAR/depth.
- Salvataggio locale di una configurazione base della stanza.
- Audio feedback o piccole animazioni quando un target viene riconosciuto.

## Architettura del progetto Unity

Scene principali:

- `MainARScene`
  - `AR Session`
  - `XR Origin`
  - `AR Camera`
  - `AR Plane Manager`
  - `AR Raycast Manager`
  - `AR Tracked Image Manager`
  - Manager custom per logica applicativa e UI.

Script consigliati:

- `ARPlacementController`
  - Gestisce tap, raycast e posizionamento dei prefab sui piani.

- `TrackedImageContentController`
  - Associa ogni immagine target al contenuto AR corretto.
  - Aggiorna posizione, rotazione e visibilita' dei prefab in base allo stato del tracking.

- `PlaneVisualizationController`
  - Mostra/nasconde mesh dei piani.
  - Distingue piani orizzontali e verticali se necessario.

- `SessionUIController`
  - Mostra stato tracking, pulsante reset, pulsante show/hide planes.

- `DemoScenarioController`
  - Coordina il flusso demo: scansione, target riconosciuti, contenuti mostrati.

Asset:

- 4-6 immagini target in `Assets/AR/ReferenceImages`.
- Prefab AR in `Assets/AR/Prefabs`.
- Materiali e icone UI in `Assets/UI`.
- Eventuali modelli 3D in `Assets/Models`.

## Fasi operative

### Fase 1 - Setup progetto

- Creare progetto Unity 3D URP.
- Installare pacchetti:
  - AR Foundation;
  - ARKit XR Plugin;
  - ARCore XR Plugin;
  - XR Plugin Management;
  - eventuale Input System se usato dal template.
- Abilitare iOS e Android come piattaforme di build.
- Configurare Player Settings per iOS:
  - Camera Usage Description;
  - target iOS compatibile con ARKit;
  - orientation portrait o landscape, da decidere subito e mantenere coerente.
- Configurare Player Settings per Android:
  - Minimum API Level compatibile con ARCore;
  - ARCore support richiesto o opzionale in base alla strategia di distribuzione;
  - permesso camera;
  - orientation coerente con iOS.
- Creare scena AR minima e verificare build su iPhone e Android.

Deliverable: app vuota AR Foundation che apre la camera e avvia una sessione AR su iOS e Android.

### Fase 2 - Plane detection e placement

- Aggiungere `AR Plane Manager` e prefab per visualizzazione piani.
- Implementare tap-to-place con `ARRaycastManager`.
- Creare un primo prefab contenuto, per esempio una dashboard 3D da scrivania.
- Aggiungere pulsante reset e pulsante show/hide planes.

Deliverable: l'utente rileva una scrivania reale e posiziona un contenuto stabile.

### Fase 3 - Image tracking

- Scegliere 4-6 immagini target reali:
  - copertina libro;
  - foglio stampato con grafica ad alto contrasto;
  - poster;
  - appunto/schema;
  - eventuale logo o segnale.
- Creare `XR Reference Image Library`.
- Associare un prefab diverso a ogni target.
- Testare condizioni di luce, distanza, angolazione e dimensione fisica delle immagini.

Deliverable: i target vengono riconosciuti nel mondo reale e mostrano contenuti AR dedicati.

### Fase 4 - Esperienza utente e scenario demo

- Definire uno scenario chiaro di demo:
  1. avvio app;
  2. scansione della stanza;
  3. posizionamento dashboard sulla scrivania;
  4. riconoscimento di 4-6 target;
  5. interazione con almeno un contenuto;
  6. reset o cambio target.
- Rifinire UI minima:
  - stato tracking;
  - istruzioni brevi;
  - reset;
  - toggle piani.
- Aggiungere feedback visuali semplici: highlight target, animazioni leggere, transizioni.

Deliverable: flusso demo completo e comprensibile senza spiegazioni esterne.

### Fase 5 - Test reale

Preparare una matrice di test:

| Test | Ambiente | Feature | Esito atteso |
| --- | --- | --- | --- |
| Piano scrivania | Stanza reale | Plane detection | Piano rilevato e contenuto stabile |
| Parete | Stanza reale | Vertical plane | Bacheca o contenuto allineato |
| Target 1-6 | Stanza reale | Image tracking | Ogni target mostra il prefab corretto |
| Luce bassa/media/alta | Stanza reale | Tracking | App resta usabile |
| Distanza 30-150 cm | Stanza reale | Image tracking | Target riconosciuti a distanze realistiche |
| Reset sessione | Stanza reale | UX | Scena pulita e riutilizzabile |

Annotare nel report:

- dispositivo usato;
- versione iOS e modello iPhone;
- versione Android e modello smartphone;
- versione Unity;
- condizioni ambientali;
- target usati;
- problemi osservati e soluzioni.

### Fase 6 - Video finale

Registrare un video di 1-3 minuti con:

- breve inquadratura dell'ambiente reale;
- avvio app su smartphone;
- rilevamento piani;
- posizionamento contenuto;
- riconoscimento di tutti i target;
- interazione con almeno un elemento;
- eventuale reset finale.

Il video deve dimostrare che il progetto e' testato nel mondo reale e non tramite simulazione.

### Fase 7 - Repository e pulizia

Preparare repository con:

- progetto Unity pulito;
- `README.md` con setup, requisiti e istruzioni build;
- cartella `docs` per report, immagini target e note di test;
- esclusione di file generati inutili tramite `.gitignore` per Unity;
- eventuale link al video nel README o report.

Non includere:

- `Library/`;
- `Temp/`;
- `Obj/`;
- build generate pesanti;
- cache locali.

### Fase 8 - Report finale

Struttura consigliata:

1. Introduzione e obiettivo del progetto.
2. Requisiti del corso soddisfatti.
3. Scelta SDK/librerie: Unity, AR Foundation, ARKit XR Plugin, ARCore XR Plugin.
4. Descrizione ambiente reale di test.
5. Design dell'esperienza AR.
6. Implementazione:
   - plane detection;
   - raycast placement;
   - image tracking;
   - gestione contenuti;
   - UI.
7. Test:
   - dispositivo;
   - immagini target;
   - condizioni;
   - risultati.
8. Problemi incontrati e soluzioni.
9. Limiti e possibili sviluppi futuri.
10. Link repository.
11. Link video.

## Timeline consigliata

| Giorno | Obiettivo |
| --- | --- |
| 1 | Setup Unity, AR Foundation, scena AR minima, build su iPhone e Android |
| 2 | Plane detection e tap-to-place |
| 3 | Image tracking con primi target |
| 4 | Contenuti AR, prefab e UI |
| 5 | Scenario demo completo |
| 6 | Test reali e correzioni |
| 7 | Video, README, pulizia repository |
| 8 | Report finale e controllo requisiti |

## Checklist finale requisiti

- [ ] App AR per smartphone.
- [ ] Compatibilita' iOS e Android.
- [ ] Uso ARKit su iOS tramite AR Foundation e ARKit XR Plugin.
- [ ] Uso ARCore su Android tramite AR Foundation e ARCore XR Plugin.
- [ ] Test nel mondo reale.
- [ ] Nessun digital twin come validazione principale.
- [ ] Ambiente aumentato tipo room/office.
- [ ] Plane detection funzionante.
- [ ] Image tracking con almeno 4-6 target.
- [ ] Augmentations utili/interessanti/divertenti.
- [ ] Interazione utente minima.
- [ ] Video demo registrato.
- [ ] Repository pulito disponibile.
- [ ] Report finale con scelte tecniche e risultati.
- [ ] Consegna almeno 5 giorni lavorativi prima dell'esame.

## Rischi principali e mitigazioni

| Rischio | Mitigazione |
| --- | --- |
| Mancanza di dispositivi compatibili | Verificare subito modello iPhone, modello Android, supporto ARKit e supporto ARCore |
| Differenze tra ARKit e ARCore | Usare solo feature comuni come plane detection, raycast e image tracking; documentare eventuali differenze |
| Build iOS/Android complessa | Fare una prima build per entrambe le piattaforme gia' in Fase 1 |
| Image target poco riconoscibili | Usare immagini ricche di dettagli, buon contrasto e dimensione fisica corretta |
| Tracking instabile in luce scarsa | Testare con illuminazione controllata e documentare limiti |
| Scope troppo ampio | Rendere obbligatorie solo plane detection, placement e image tracking; lasciare occlusion/persistenza come extra |
| File `notes.pdf` mancante | Aggiornare il piano appena il file viene aggiunto al workspace |
