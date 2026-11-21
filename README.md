# Full Caster Merge

Mod de Unity Mod Manager para Pathfinder: Wrath of the Righteous.

Funde o spellbook de um caminho mítico no spellbook de qualquer classe. O livro fundido deixa de usar o teto da classe e passa a usar a progressão de um conjurador completo.

## O que muda

- No menu do UMM, com um save carregado, cada personagem lista o livro da classe e o livro mítico separado. O botão funde os dois.
- Caster level continua sendo o da classe mais o mythic rank, como no merge do jogo e do Toybox.
- Se o livro da classe parava antes do 9º círculo, os slots de círculo 1 a 9 passam a seguir a tabela de um full caster. Livro espontâneo usa a tabela de feiticeiro/oráculo. Livro preparado usa a de mago/clérigo.
- A lista de magias da classe não é trocada. Um Inquisitor não aprende a lista de Clérigo. O que entra nos círculos novos são as magias do livro mítico.
- Um livro que já era full caster, como Clérigo ou Mago, não tem a tabela substituída.
- O 10º círculo só abre se o personagem tiver a feature do jogo que libera magia de nível 10. Sem isso, o teto é o 9º.
- Um merge já feito pelo Toybox também recebe essa progressão ao carregar o save. Não precisa fundir de novo.

## Instalação

1. Instale o Unity Mod Manager e aplique no Wrath of the Righteous.
2. Compile com `dotnet build`, ou copie `FullCasterMerge.dll` e `Info.json` para `<jogo>\Mods\FullCasterMerge`.
3. Abra o jogo, confirme o mod no UMM e carregue o save.
4. Salve antes de fundir. O merge fica no personagem e este menu não desfaz.

O build copia o mod para a pasta Mods do jogo.

## Limites

Desligar o mod não desfaz o merge. Os círculos extras somem enquanto o mod estiver desligado, porque a tabela da classe continua a original, e voltam quando o mod é ligado de novo.

Círculos novos de um livro espontâneo começam cheios se o pool daquele círculo estiver em zero. Descansar no jogo também enche o pool. Livro preparado ganha slots vazios para memorizar.
