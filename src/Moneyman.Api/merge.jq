.[1][0] as $two
| .[0]
| reduce range(0; length) as $i (.;
    .[$i].baz = $two[$i] )
