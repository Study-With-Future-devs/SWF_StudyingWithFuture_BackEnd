SELECT 
    u.Id,
    u.Nome,
    u.Email,
    CASE 
        WHEN a.Id IS NOT NULL THEN 'Admin'
        WHEN al.Id IS NOT NULL THEN 'Aluno'
        WHEN p.Id IS NOT NULL THEN 'Professor'
        WHEN c.Id IS NOT NULL THEN 'Coordenador'
    END as TipoUsuario,
    t.Nome as Tela,
    t.Descricao as DescricaoTela
FROM Usuarios u
LEFT JOIN Admins a ON u.Id = a.Id
LEFT JOIN Alunos al ON u.Id = al.Id
LEFT JOIN Professores p ON u.Id = p.Id
LEFT JOIN Coordenadores c ON u.Id = c.Id
INNER JOIN UsuarioTelas ut ON u.Id = ut.UsuarioId
INNER JOIN Telas t ON ut.TelaId = t.Id;