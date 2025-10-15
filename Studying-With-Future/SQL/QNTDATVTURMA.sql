SELECT 
    t.Id,
    t.Codigo,
    t.Descricao,
    COUNT(ativ.Id) as QuantidadeAtividades
FROM Turmas t
LEFT JOIN Atividades ativ ON t.Id = ativ.TurmaId
GROUP BY t.Id, t.Codigo, t.Descricao;