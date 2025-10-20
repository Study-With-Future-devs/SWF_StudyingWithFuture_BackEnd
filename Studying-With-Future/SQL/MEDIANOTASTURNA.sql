SELECT 
    t.Id,
    t.Codigo,
    t.Descricao,
    AVG(n.Valor) AS MediaNotas
FROM Turmas t
INNER JOIN Atividades ativ ON t.Id = ativ.TurmaId
INNER JOIN Notas n ON ativ.Id = n.AtividadeId
GROUP BY t.Id, t.Codigo, t.Descricao;
