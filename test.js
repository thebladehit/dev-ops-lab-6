// const a = (array) => {
//   let x = array[0];
//   let y = Math.floor(x / 2);

//   for (let i = 1; i < array.length; i++) {
//     if (array[i] > y) {
//       x = array[i - 1];
//       y = x + 2;
//     }

//     if (x % 2 == 0) {
//       x++;
//     }
//   }

//   return x + y;
// }

// array = [5, 2, 9, 3, 7, 6];
// console.log(a(array))

// const a = (grid) => {
//   let result = 0;
//   let rows = grid.length;
//   let cols = grid[0].length;
//   let visited = new Array(rows);
//   for (let i = 0; i < rows; i++) {
//     visited[i] = new Array(cols);
//     visited[i].fill(false);
//   }

//   function dfs(row, col) {
//     if (row < 0 || col < 0 || row >= rows || col >= cols || grid[row][col] == '0' || visited[row][col]) {
//       return;
//     }

//     visited[row][col] = true;

//     dfs(row - 1, col);
//     dfs(row + 1, col);
//     dfs(row, col - 1);
//     dfs(row, col + 1);
//   }

//   for (let r = 0; r < rows; r++) {
//     for (let c = 0; c < cols; c++) {
//       if (grid[r][c] == '1' && !visited[r][c]) {
//         result += 1;
//         dfs(r, c);
//       }
//     }
//   }
//   return result;
// };

// const grid = [
//   ['1', '1', '1', '0', '0'],
//   ['1', '1', '0', '0', '0'],
//   ['0', '0', '1', '0', '0'],
//   ['0', '0', '0', '1', '1'],
//   ['0', '1', '0', '1', '0'],
// ]
// console.log(a(grid));


// function a(prices) {
//   if (prices.length < 2) return;

//   let minPrice = prices[0];
//   let maxProfit = 0;

//   for (let i =1; i < prices.length; i++) {
//     minPrice = Math.min(minPrice, prices[i]);

//     maxProfit = Math.max(maxProfit, prices[i] -minPrice);
//   }

//   return maxProfit
// }

// prices = [7, 1, 5, 3, 6, 4]

// console.log(a(prices))

console.log(5 / 2)