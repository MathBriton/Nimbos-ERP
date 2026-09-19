// @ts-check
/**
 * Copia os temas do PrimeReact de node_modules para public/temas/.
 *
 * Por que copiar em vez de importar:
 * o theme.css referencia as fontes por caminho relativo ("./fonts/..."). Um
 * import com "?url" do Vite emitiria o CSS como asset sem reescrever essas
 * URLs, e as fontes quebrariam. Servindo a pasta inteira a partir de public/,
 * os caminhos relativos continuam valendo.
 *
 * As pastas de destino sao nomeadas "claro" e "escuro", nao com os nomes do
 * PrimeReact: assim tanto o script inline do index.html quanto o provider do
 * React montam a URL como `/temas/${tema}/theme.css`, sem nenhum mapa de nomes
 * duplicado entre os dois lugares.
 *
 * As fontes sao identicas nos dois temas, entao ficam em uma pasta unica e o
 * CSS e reescrito para apontar para la. Alem de economizar ~700 KB, evita que
 * o navegador baixe as fontes de novo a cada troca de tema.
 */

import { cp, mkdir, readFile, rm, writeFile } from 'node:fs/promises';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const raizDoFrontend = join(dirname(fileURLToPath(import.meta.url)), '..');
const origemDosTemas = join(raizDoFrontend, 'node_modules/primereact/resources/themes');
const destino = join(raizDoFrontend, 'public/temas');

/** Tema do PrimeReact usado para cada modo do Nimbus. */
const temas = {
  claro: 'lara-light-indigo',
  escuro: 'lara-dark-indigo',
};

async function copiarTemas() {
  // Comeca do zero: sobras de uma execucao anterior poderiam mascarar erro.
  await rm(destino, { recursive: true, force: true });
  await mkdir(destino, { recursive: true });

  const nomesDosTemas = Object.entries(temas);
  const [, primeiroTema] = nomesDosTemas[0];

  // As fontes sao iguais em todos os temas: uma copia so serve a todos.
  await cp(join(origemDosTemas, primeiroTema, 'fonts'), join(destino, 'fontes'), {
    recursive: true,
  });

  for (const [modo, temaDoPrimeReact] of nomesDosTemas) {
    const pasta = join(destino, modo);
    await mkdir(pasta, { recursive: true });

    const css = await readFile(join(origemDosTemas, temaDoPrimeReact, 'theme.css'), 'utf8');

    // "./fonts/" e relativo ao CSS (public/temas/<modo>/); a pasta unica de
    // fontes fica um nivel acima, em public/temas/fontes/.
    const cssReescrito = css.replaceAll('./fonts/', '../fontes/');

    if (cssReescrito === css && css.includes('fonts/')) {
      throw new Error(
        `Nao foi possivel reescrever os caminhos de fonte de '${temaDoPrimeReact}'. ` +
          'O formato do tema mudou; ajuste scripts/copiar-temas.mjs.',
      );
    }

    await writeFile(join(pasta, 'theme.css'), cssReescrito, 'utf8');

    console.log(`tema '${modo}' copiado de ${temaDoPrimeReact}`);
  }
}

await copiarTemas();
